#region

using System.Diagnostics;
using NUnit.Framework;
using RazorSharp;
using RazorSharp.Memory;
using RazorSharp.Pointers;

#endregion

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

		[Test]
		public void Test2()
		{
			const int INT_SIZE = 4;
			const int LENGTH   = 10;
			const int SIZE     = INT_SIZE * LENGTH;

			Pointer<int> ptr = AllocPool.Alloc<int>(LENGTH);

			for (int i = 0; i < AllocPool.GetLength(ptr); i++) {
//				AllocPool.Info(ptr);
//				Console.WriteLine(ptr.Query());
//				Thread.Sleep(1000);
//				Console.Clear();

				Debug.Assert(AllocPool.GetLength(ptr) == LENGTH);
				Debug.Assert(AllocPool.GetSize(ptr) == SIZE);
				Debug.Assert(AllocPool.IsAllocated(ptr));
				Debug.Assert(AllocPool.GetOffset(ptr) == i);


				if (AllocPool.GetOffset(ptr) + 1 < AllocPool.GetLength(ptr)) ptr++;
			}

			AllocPool.Free(ptr);
		}
	}
}