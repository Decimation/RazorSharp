#region

using RazorSharp.Memory;
using RazorSharp.Memory.Pointers;
using CSUnsafe = System.Runtime.CompilerServices.Unsafe;

// ReSharper disable ReturnTypeCanBeEnumerable.Global

#endregion

namespace RazorSharp.Utilities
{
	// todo: WIP

	public static unsafe class Conversions
	{
		public static TTo[] ConvertArray<TTo>(byte[] mem)
		{
			fixed (byte* ptr = mem) {
				Pointer<TTo> memPtr = ptr;
				return memPtr.CopyOut(mem.Length / memPtr.ElementSize);
			}
		}

		public static TTo Convert<TFrom, TTo>(TFrom t, ConversionType c = ConversionType.REINTERPRET)
		{
			switch (c) {
				case ConversionType.REINTERPRET:
					return ReinterpretCast<TFrom, TTo>(t);
				case ConversionType.LIGHT:
					return (TTo) System.Convert.ChangeType(t, typeof(TTo));
				case ConversionType.AS:
					return CSUnsafe.As<TFrom, TTo>(ref t);
				default:
					return default;
			}
		}

		public static TTo Convert<TTo>(byte[] mem) where TTo : struct
		{
			Pointer<byte> alloc = Mem.AllocUnmanaged<byte>(mem.Length);
			alloc.WriteAll(mem);
			var read = alloc.ReadAny<TTo>();
			Mem.Free(alloc);
			return read;
		}


		/// <summary>
		///     Reinterprets <paramref name="value" /> of type <typeparamref name="TFrom" /> as a value of type
		///     <typeparamref name="TTo" />
		/// </summary>
		/// <param name="value">Value to reinterpret</param>
		/// <typeparam name="TFrom">Inherent type</typeparam>
		/// <typeparam name="TTo">Type to reinterpret <typeparamref name="TFrom" /> as</typeparam>
		/// <returns></returns>
		public static TTo ReinterpretCast<TFrom, TTo>(TFrom value)
		{
			Pointer<TFrom> addr = Unsafe.AddressOf(ref value);
			return addr.ReadAny<TTo>();
		}
	}
}