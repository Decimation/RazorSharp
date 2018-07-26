using System;
using System.Runtime.InteropServices;
using NUnit.Framework;
using RazorSharp;

namespace Test.Testing.Tests
{
	using CSUnsafe = System.Runtime.CompilerServices.Unsafe;

	[TestFixture]
	internal unsafe class UnsafeTests
	{
		[Test]
		public void Test()
		{
			int valType = 0xFF;
			Assert.That(new IntPtr(&valType), Is.EqualTo(Unsafe.AddressOf(ref valType)));

			string s = "foo";
			Assert.That(new IntPtr(CSUnsafe.AsPointer(ref s)), Is.EqualTo(Unsafe.AddressOf(ref s)));

			IntPtr sChars = Unsafe.AddressOfHeap(ref s, OffsetType.StringData);
			Assert.That(Marshal.ReadInt16(sChars), Is.EqualTo(s[0]));

			int[] arr = {1, 2, 3};
			IntPtr arrData = Unsafe.AddressOfHeap(ref arr, OffsetType.ArrayData);
			Assert.That(Marshal.ReadInt32(arrData), Is.EqualTo(arr[0]));

			Dummy d = new Dummy(100, "bar");
			IntPtr dData = Unsafe.AddressOfHeap(ref d, OffsetType.Fields);
			// Largest field is first in memory
			Assert.That(Marshal.ReadInt32(dData, IntPtr.Size), Is.EqualTo(100));
		}
	}

}