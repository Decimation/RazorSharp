#region

using RazorSharp.Memory;
using RazorSharp.Pointers;

#endregion

namespace RazorSharp
{
	// todo: WIP
	public static class RazorConvert
	{
		public enum ConversionType
		{
			LOW_LEVEL
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