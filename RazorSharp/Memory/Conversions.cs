#region

using System;
using RazorSharp.CoreClr;
using RazorSharp.CoreClr.Metadata;
using RazorSharp.Interop;
using RazorSharp.Memory.Pointers;

#endregion

namespace RazorSharp.Memory
{
	#region

	
	

	#endregion

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
		/// <see cref="System.Convert.ChangeType(object,Type)"/>
		/// </summary>
		Normal,
		
		/// <summary>
		/// <see cref="Unsafe.As{T,T}"/>
		/// </summary>
		Proxy
	}
	
	public static unsafe class Conversions
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
					return Memory.Unsafe.AddressOf(ref t).Cast<TTo>().Read();
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
		///     only contain its fields. This memory does not need to be freed.
		/// </summary>
		/// <param name="mem">Memory to load from</param>
		/// <typeparam name="T">Type to load</typeparam>
		/// <returns>An instance created from <paramref name="mem" /></returns>
		public static T AllocLoad<T>(byte[] mem)
		{
			T value = default;

			Pointer<byte> addr;

			if (RuntimeInfo.IsStruct<T>()) {
				addr = Memory.Unsafe.AddressOf(ref value).Cast();
			}
			else {
				value = Runtime.AllocObject<T>();
				addr  = Memory.Unsafe.AddressOfFields(ref value).Cast();
			}

			addr.WriteAll(mem);

			return value;
		}

		public static object AllocLoad(byte[] mem, Type type)
		{
			return Functions.CallGenericMethod(typeof(Conversions), nameof(AllocLoad),
			                                          null, new[] {type}, mem);
		}
	}
}