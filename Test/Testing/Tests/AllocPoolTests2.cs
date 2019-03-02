using System;
using System.Linq;
using RazorSharp.Memory;
using RazorSharp.Pointers;
using RazorCommon;
namespace Test.Testing.Tests
{
	public class AllocPoolTests2
	{
		public static void Test()
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
	}
}