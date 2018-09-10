using System.Diagnostics;
using NUnit.Framework;
using RazorSharp;
using RazorSharp.Memory;
using RazorSharp.Pointers;

namespace Test.Testing.Tests
{

	[TestFixture]
	public class AllocPoolTests
	{
		[Test]
		public void Test()
		{
			const int    alloc = 3;
			Pointer<int> ptr   = AllocPool.Alloc<int>(alloc);

			for (int i = 0; i < alloc; i++) {
				Debug.Assert(AllocPool.IsAllocated(ptr));
				Debug.Assert(AllocPool.GetLength(ptr) == alloc);
				Debug.Assert(AllocPool.GetSize(ptr) == alloc * sizeof(int));
				Debug.Assert(AllocPool.GetOffset(ptr) == i);
				ptr++;
			}

			--ptr;
			ptr = AllocPool.ReAlloc(ptr, AllocPool.GetLength(ptr) * 2);
			Debug.Assert(AllocPool.GetOffset(ptr) == 0);
			Debug.Assert(AllocPool.GetLength(ptr) == 6);
			Debug.Assert(AllocPool.GetSize(ptr) == AllocPool.GetLength(ptr) * Unsafe.SizeOf<int>());
			Debug.Assert(AllocPool.IsAllocated(ptr));
			AllocPool.Free(ptr);
			Debug.Assert(!AllocPool.IsAllocated(ptr));
		}
	}

}