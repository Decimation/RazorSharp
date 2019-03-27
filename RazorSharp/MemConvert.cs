#region

using RazorSharp.Memory;
using RazorSharp.Pointers;
using System;
using System.Runtime.InteropServices;
using CSUnsafe = System.Runtime.CompilerServices.Unsafe;
#endregion

namespace RazorSharp
{
	// todo: WIP

	public static class MemConvert
	{
		public enum ConversionType
		{
			LOW_LEVEL,
			LIGHT,
			AS
		}

		public static unsafe TTo[] ConvertArray<TTo>(byte[] mem)
		{
			fixed (byte* ptr = mem) {
				Pointer<TTo> memPtr = ptr;
				return memPtr.CopyOut(mem.Length / memPtr.ElementSize);
			}
		}
		
		public static TTo Convert<TFrom, TTo>(TFrom t, ConversionType c = ConversionType.LOW_LEVEL)
		{
			switch (c) {
				case ConversionType.LOW_LEVEL:
					return ProxyCast<TFrom, TTo>(t);
				case ConversionType.LIGHT:
					return (TTo) System.Convert.ChangeType(t, typeof(TTo));
				case ConversionType.AS:
					//return CSUnsafe.As<TFrom, TTo>(ref t);
				default:
					return default;
			}

			return default;
		}

		public static TTo Convert<TTo>(byte[] mem) where TTo : struct
		{
			Pointer<byte> alloc = Mem.AllocUnmanaged<byte>(mem.Length);
			alloc.WriteAll(mem);
			var read = alloc.ReadAny<TTo>();
			Mem.Free(alloc);
			return read;
		}

		public static TProxy ProxyCast<TOld, TProxy>(TOld value)
		{
			return Unsafe.AddressOf(ref value).ReadAs<TOld, TProxy>();
		}
	}
}