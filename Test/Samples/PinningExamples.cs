using RazorSharp;
using RazorSharp.Memory.Fixed;
using RazorSharp.Pointers;

namespace Test.Samples
{
	public static class PinningExamples
	{
		internal static void SetChar(this string str, int i, char c)
		{
			ObjectPinner.InvokeWhilePinned(str, delegate
			{
				Pointer<char> lpChar = Unsafe.AddressOfHeap(ref str, OffsetType.StringData).Address;
				lpChar[i] = c;
			});
		}

		internal static void Set(this string str, string s)
		{
			ObjectPinner.InvokeWhilePinned(str, delegate
			{
				Pointer<char> lpChar = Unsafe.AddressOfHeap(ref str, OffsetType.StringData).Address;
				lpChar.WriteAll(s);
			});
		}
	}
}