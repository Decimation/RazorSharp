using RazorSharp.CoreClr;
using RazorSharp.Memory.Pointers;

namespace RazorSharp.Memory
{
	public static class Cleanup
	{
		public static void Destroy<T>(ref T value)
		{
			if (!RtInfo.IsStruct(value)) {
				int           size = Unsafe.SizeOfData(value);
				Pointer<byte> ptr  = Unsafe.AddressOfData(ref value);
				ptr.ZeroBytes(size);
			}
			else {
				value = default;
			}
		}
	}
}