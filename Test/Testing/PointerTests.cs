using System;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using RazorSharp;
using RazorSharp.Pointers;
using RazorSharp.Runtime;

namespace Test.Testing
{

	[TestFixture]
	internal unsafe class PointerTests
	{
		[Test]
		public void Test()
		{
			string        x = "foo";
			string        y = "bar";
			Pointer<char> p = x;
			Assertion.AssertElements(p, x);
			p = y;
			Assertion.AssertElements(p, y);

			int[]        arr = {1, 2, 3};
			Pointer<int> p2  = arr;
			Assertion.AssertElements(p2, arr);


			string        z     = "anime";
			Pointer<char> chPtr = z;

			Assert.That(chPtr[0], Is.EqualTo(z[0]));
			chPtr++;
			Assert.That(chPtr[0], Is.EqualTo(z[1]));

			//AssertElements(chPtr, z);
		}


	}

}