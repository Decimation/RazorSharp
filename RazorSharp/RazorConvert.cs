using RazorSharp.Memory;

namespace RazorSharp
{
	// todo: WIP
	public static class RazorConvert
	{
		public enum ConversionType
		{
			LOW_LEVEL
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
			var alloc = Mem.AllocUnmanaged<byte>(mem.Length);
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