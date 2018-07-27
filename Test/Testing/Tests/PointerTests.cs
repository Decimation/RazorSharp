using System.Diagnostics;
using NUnit.Framework;
using RazorSharp.Pointers;

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
		}

		[Test]
		public void TestArrayPointers()
		{
			string        x = "foo";
			string        y = "bar";
			ArrayPointer<char> p = x;
			TestingAssertion.AssertElements(p, x);
			p = y;
			TestingAssertion.AssertElements(p, y);

			int[]        arr = {1, 2, 3};
			ArrayPointer<int> p2  = arr;
			TestingAssertion.AssertElements(p2, arr);


			string        z     = "anime";
			ArrayPointer<char> chPtr = z;

			Assert.That(chPtr[0], Is.EqualTo(z[0]));
			chPtr++;
			Assert.That(chPtr[0], Is.EqualTo(z[1]));
			chPtr--;

			ArrayPointer<char> chPtr2 = z;

			Debug.Assert(chPtr == chPtr2);


			//AssertElements(chPtr, z);
		}


	}

}