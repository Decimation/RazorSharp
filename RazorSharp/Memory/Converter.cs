#region

using System;
using RazorSharp.CoreClr;
using RazorSharp.CoreClr.Metadata;
using RazorSharp.Interop;
using RazorSharp.Memory.Enums;
using RazorSharp.Memory.Pointers;

#endregion

namespace RazorSharp.Memory
{
	#region

	/// <summary>
	/// Specifies how a value will be converted
	/// </summary>
	public enum ConversionType
	{
		/// <summary>
		///     Reinterprets a value as a value of the specified conversion type
		/// </summary>
		Reinterpret,

		/// <summary>
		/// <see cref="Convert.ChangeType(object,System.Type)"/>
		/// </summary>
		Normal,

		/// <summary>
		/// <see cref="Unsafe.As{T,T}"/>
		/// </summary>
		Proxy
	}
	
	#endregion

	/// <summary>
	/// <seealso cref="System.Convert"/>
	/// <seealso cref="BitConverter"/>
	/// </summary>
	public static unsafe class Converter
	{
		public static T ToObject<T>(Pointer<byte> ptr)
		{
			void* cpy = ptr.ToPointer();
			return Unsafe.Read<T>(&cpy);
		}

		public static TTo[] ConvertArray<TTo>(byte[] mem)
		{
			fixed (byte* ptr = mem) {
				Pointer<TTo> memPtr = ptr;
				return memPtr.Copy(mem.Length / memPtr.ElementSize);
			}
		}

		public static TTo Convert<TFrom, TTo>(TFrom t, ConversionType c = ConversionType.Reinterpret)
		{
			switch (c) {
				case ConversionType.Reinterpret:
					return Unsafe.AddressOf(ref t).Cast<TTo>().Read();
				case ConversionType.Normal:
					return (TTo) System.Convert.ChangeType(t, typeof(TTo));
				case ConversionType.Proxy:
					return Unsafe.As<TFrom, TTo>(ref t);
				default:
					return default;
			}
		}

		public static TTo Convert<TTo>(byte[] mem) /*where TTo : struct*/
		{
			fixed (byte* bptr = mem) {
				Pointer<TTo> ptr = bptr;
				return ptr.Read();
			}
		}

		/// <summary>
		///     Creates an instance of <typeparamref name="T" /> from <paramref name="mem" />.
		///     If <typeparamref name="T" /> is a reference type, <paramref name="mem" /> should not contain
		///     object internals like its <see cref="MethodTable" /> pointer or its <see cref="ObjHeader" />; it should
		///     only contain its fields. This memory does not need to be freed, as it is GC allocated.
		/// </summary>
		/// <param name="mem">Memory to load from</param>
		/// <typeparam name="T">Type to load</typeparam>
		/// <returns>An instance created from <paramref name="mem" /></returns>
		public static T AllocRaw<T>(byte[] mem)
		{
			T value = default;

			Pointer<byte> addr;

			if (Runtime.Info.IsStruct<T>()) {
				addr = Unsafe.AddressOf(ref value).Cast();
			}
			else {
				value = Runtime.AllocObject<T>();
				addr  = Unsafe.AddressOfFields(ref value).Cast();
			}

			addr.WriteAll(mem);

			return value;
		}

		public static object AllocRaw(byte[] mem, Type type)
		{
			return Functions.Reflection.CallGeneric(typeof(Converter).GetMethod(nameof(AllocRaw)),
			                                   type, null, mem);
		}
	}
}