using System.Diagnostics;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using RazorSharp;
using RazorSharp.Pointers;
using Unsafe = RazorSharp.Unsafe;

namespace Test.Testing.Tests
{

	[TestFixture]
	internal unsafe class PointerTests
	{
		[Test]
		public void Test()
		{
			string s = "foo";
			Pointer<string> strPtr = new Pointer<string>(ref s);
			Assert.That(strPtr.Value, Is.EqualTo(s));

			Pointer<string> strPtr2 = new Pointer<string>(ref s);
			Debug.Assert(strPtr == strPtr2);

			string[] arr = {"", "foo", "anime"};
			Pointer<string> strPtr3 = Unsafe.AddressOfHeap(ref arr, OffsetType.ArrayData);
			RAssert.Elements(strPtr3, arr);
			strPtr3 -= 3;
			Debug.Assert(strPtr3.Value == arr[0]);
			strPtr3++;
			Debug.Assert(strPtr3.Value == arr[1]);
		}

		[Test]
		public void TestArrayPointers()
		{
			string        x = "foo";
			string        y = "bar";
			DecayPointer<char> p = x;
			RAssert.Elements(p, x);
			p = y;
			RAssert.Elements(p, y);

			int[]        arr = {1, 2, 3};
			DecayPointer<int> p2  = arr;
			RAssert.Elements(p2, arr);


			string        z     = "anime";
			DecayPointer<char> chPtr = z;

			Assert.That(chPtr[0], Is.EqualTo(z[0]));
			chPtr++;
			Assert.That(chPtr[0], Is.EqualTo(z[1]));
			chPtr--;

			DecayPointer<char> chPtr2 = z;

			Debug.Assert(chPtr == chPtr2);


			//AssertElements(chPtr, z);
		}


	}

}