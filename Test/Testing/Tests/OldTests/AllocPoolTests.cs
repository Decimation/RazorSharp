#region

using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using RazorCommon;
using RazorSharp;
using RazorSharp.Memory;
using RazorSharp.Pointers;

#endregion

namespace Test.Testing.Tests
{
	[TestFixture]
	public class AllocPoolTests
	{
		public void AltTest()
		{
			Pointer<string> alloc = AllocPool.Alloc<string>(10);
			alloc.WriteAll("foo", "bar");
			Console.WriteLine(alloc.ToTable(10).ToMarkDownString());
			alloc.Init(AllocPool.GetLength(alloc));
			GC.Collect();
			Console.WriteLine(alloc.ToTable(10).ToMarkDownString());
			AllocPool.Free(alloc);
			GC.Collect();

			Array rg = new long[1];
			rg.SetValue(Mem.ReinterpretCast<string, long>("foo"), 0);

			Pointer<int> iAlloc = AllocPool.Alloc<int>(10);
			iAlloc.Set(0xFF, AllocPool.GetLength(iAlloc));
			Console.WriteLine(iAlloc.ToTable(10).ToMarkDownString());
			iAlloc.Set(int.MaxValue, 1, 9);
			Console.WriteLine(iAlloc.ToTable(10).ToMarkDownString());
			AllocPool.Free(iAlloc);

			Pointer<uint> sPtr = AllocPool.Alloc<uint>(2);
			sPtr.WriteAny("foo");
			Console.WriteLine(sPtr.ToTable(2).ToMarkDownString());
			Console.WriteLine(sPtr.ReadAny<string>());
			Console.WriteLine(sPtr.Reinterpret<string>().CopyOut(1).Join());
			AllocPool.Free(sPtr);


			Pointer<int> integers = AllocPool.Alloc<int>(10);
			Console.WriteLine(integers.ToTable(10).ToMarkDownString());
			integers.Set(0xFF, 10);
			Console.WriteLine(integers.ToTable(10).ToMarkDownString());
			integers.Set(int.MaxValue, 1, 9);
			Console.WriteLine(integers.ToTable(10).ToMarkDownString());
			Console.WriteLine(integers.Where(10, x => x > 1).ToArray().Join());
			AllocPool.Free(integers);
		}

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