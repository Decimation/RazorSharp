using System;
using System.Diagnostics;
using NUnit.Framework;
using RazorSharp.CLR;
using RazorSharp.Memory;
using RazorSharp.Pointers;

namespace Test.Testing.Tests
{

	[TestFixture]
	public class AddrInRangeTests
	{
		[Test]
		public void Test()
		{
			Debug.Assert(Memory.IsAddressInRange(Memory.StackBase, Memory.StackLimit, Memory.StackLimit));
			Debug.Assert(Memory.IsAddressInRange(GCHeap.HighestAddress, GCHeap.LowestAddress, GCHeap.LowestAddress));
			int[]        rg    = {1, 2, 3, 4, 5};
			Pointer<int> rgPtr = Memory.AllocUnmanaged<int>(5);
			rgPtr.Init(1, 2, 3, 4, 5);

			Debug.Assert(rgPtr.Contains(1, 5));

			Memory.Free(rgPtr.Address);

			IntPtr orig = rgPtr.Address;

			for (int i = 0; i < 5; i++) {
				Debug.Assert(InRange(rgPtr.Address));

				rgPtr++;
			}


			Debug.Assert(!InRange(rgPtr.Address));


			bool InRange(IntPtr p)
			{
				return Memory.IsAddressInRange(PointerUtils.Offset<int>(orig, 5), p, orig);
			}
		}
	}

}