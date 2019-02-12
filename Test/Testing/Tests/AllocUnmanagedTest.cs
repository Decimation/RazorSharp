using System;
using System.Diagnostics;
using RazorSharp.Memory;
using RazorSharp.Pointers;

namespace Test.Testing.Tests
{
	public class AllocUnmanagedTest
	{
		public static void Test()
		{
			#region Alloc unmanaged test

			Pointer<string> mptr = Mem.AllocUnmanaged<string>(3);
			string[]        rg   = {"anime", "gf", "pls"};
			mptr.WriteAll(rg);
			for (int i = 0; i < 3; i++) Debug.Assert(rg[i] == mptr[i]);

			Mem.Free(mptr);
			GC.Collect();

			#endregion
		}
	}
}