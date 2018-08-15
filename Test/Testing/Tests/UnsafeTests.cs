#region

using System;
using System.Runtime.InteropServices;
using NUnit.Framework;
using RazorSharp;
using Unsafe = System.Runtime.CompilerServices.Unsafe;

#endregion

namespace Test.Testing.Tests
{

	#region

	using CSUnsafe = Unsafe;

	#endregion

	[TestFixture]
	internal unsafe class UnsafeTests
	{
		[Test]
		public void Test()
		{
			int valType = 0xFF;
			Assert.That(new IntPtr(&valType), Is.EqualTo(RazorSharp.Unsafe.AddressOf(ref valType)));

			string s = "foo";
			Assert.That(new IntPtr(CSUnsafe.AsPointer(ref s)), Is.EqualTo(RazorSharp.Unsafe.AddressOf(ref s)));

			IntPtr sChars = RazorSharp.Unsafe.AddressOfHeap(ref s, OffsetType.StringData);
			Assert.That(Marshal.ReadInt16(sChars), Is.EqualTo(s[0]));

			int[]  arr     = {1, 2, 3};
			IntPtr arrData = RazorSharp.Unsafe.AddressOfHeap(ref arr, OffsetType.ArrayData);
			Assert.That(Marshal.ReadInt32(arrData), Is.EqualTo(arr[0]));

			//Dummy d = new Dummy(100, "bar");
			//IntPtr dData = Unsafe.AddressOfHeap(ref d, OffsetType.Fields);
			// Largest field is first in memory
			//Assert.That(Marshal.ReadInt32(dData, IntPtr.Size), Is.EqualTo(100));
		}
	}

}