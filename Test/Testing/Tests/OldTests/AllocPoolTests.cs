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
			Pointer<string> alloc = AllocHelper.Alloc<string>(10);
			alloc.WriteAll("foo", "bar");
			Console.WriteLine(alloc.ToTable(10).ToMarkDownString());
			alloc.Init(AllocHelper.GetLength(alloc));
			GC.Collect();
			Console.WriteLine(alloc.ToTable(10).ToMarkDownString());
			AllocHelper.Free(alloc);
			GC.Collect();

			Array rg = new long[1];
			rg.SetValue(Mem.ReinterpretCast<string, long>("foo"), 0);

			Pointer<int> iAlloc = AllocHelper.Alloc<int>(10);
			iAlloc.Set(0xFF, AllocHelper.GetLength(iAlloc));
			Console.WriteLine(iAlloc.ToTable(10).ToMarkDownString());
			iAlloc.Set(int.MaxValue, 1, 9);
			Console.WriteLine(iAlloc.ToTable(10).ToMarkDownString());
			AllocHelper.Free(iAlloc);

			Pointer<uint> sPtr = AllocHelper.Alloc<uint>(2);
			sPtr.WriteAny("foo");
			Console.WriteLine(sPtr.ToTable(2).ToMarkDownString());
			Console.WriteLine(sPtr.ReadAny<string>());
			Console.WriteLine(sPtr.Cast<string>().CopyOut(1).Join());
			AllocHelper.Free(sPtr);


			Pointer<int> integers = AllocHelper.Alloc<int>(10);
			Console.WriteLine(integers.ToTable(10).ToMarkDownString());
			integers.Set(0xFF, 10);
			Console.WriteLine(integers.ToTable(10).ToMarkDownString());
			integers.Set(int.MaxValue, 1, 9);
			Console.WriteLine(integers.ToTable(10).ToMarkDownString());
			Console.WriteLine(integers.Where(10, x => x > 1).ToArray().Join());
			AllocHelper.Free(integers);
		}

		[Test]
		public void Test()
		{
			const int    alloc = 3;
			Pointer<int> ptr   = AllocHelper.Alloc<int>(alloc);

			for (int i = 0; i < alloc; i++) {
				Debug.Assert(AllocHelper.IsAllocated(ptr));
				Debug.Assert(AllocHelper.GetLength(ptr) == alloc);
				Debug.Assert(AllocHelper.GetSize(ptr) == alloc * sizeof(int));
				Debug.Assert(AllocHelper.GetOffset(ptr) == i);
				ptr++;
			}

			--ptr;
			ptr = AllocHelper.ReAlloc(ptr, AllocHelper.GetLength(ptr) * 2);
			Debug.Assert(AllocHelper.GetOffset(ptr) == 0);
			Debug.Assert(AllocHelper.GetLength(ptr) == 6);
			Debug.Assert(AllocHelper.GetSize(ptr) == AllocHelper.GetLength(ptr) * Unsafe.SizeOf<int>());
			Debug.Assert(AllocHelper.IsAllocated(ptr));
			AllocHelper.Free(ptr);
			Debug.Assert(!AllocHelper.IsAllocated(ptr));
		}

		[Test]
		public void Test2()
		{
			const int INT_SIZE = 4;
			const int LENGTH   = 10;
			const int SIZE     = INT_SIZE * LENGTH;

			Pointer<int> ptr = AllocHelper.Alloc<int>(LENGTH);

			for (int i = 0; i < AllocHelper.GetLength(ptr); i++) {
//				AllocPool.Info(ptr);
//				Console.WriteLine(ptr.Query());
//				Thread.Sleep(1000);
//				Console.Clear();

				Debug.Assert(AllocHelper.GetLength(ptr) == LENGTH);
				Debug.Assert(AllocHelper.GetSize(ptr) == SIZE);
				Debug.Assert(AllocHelper.IsAllocated(ptr));
				Debug.Assert(AllocHelper.GetOffset(ptr) == i);


				if (AllocHelper.GetOffset(ptr) + 1 < AllocHelper.GetLength(ptr)) ptr++;
			}

			AllocHelper.Free(ptr);
		}
	}
}