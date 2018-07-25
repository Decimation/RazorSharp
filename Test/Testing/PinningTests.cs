using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using NUnit.Framework;
using RazorSharp;
using RazorSharp.Experimental;
using RazorSharp.Pointers;
using RazorSharp.Runtime;

namespace Test.Testing
{
	[TestFixture]
	internal class PinningTests
	{
		/*[Test]
		public void TestPinHandle()
		{
			string s = "foo";
			PinHandle<string> p = PinHandle<string>.Pin(ref s);

			Assertion.AssertPinning(ref s);

			Dummy d = new Dummy();
			Debug.Assert(!Unsafe.IsBlittable<Dummy>());
			PinHandle<Dummy> p2 = PinHandle<Dummy>.Pin(ref d);
			Assertion.AssertPinning(ref d);
			p.Unpin();
			p2.Unpin();

			Assert.That(p.IsAllocated, Is.EqualTo(false));
			Assert.That(p.IsPinned, Is.EqualTo(false));
			Assert.That(p2.IsAllocated, Is.EqualTo(false));
			Assert.That(p2.IsPinned, Is.EqualTo(false));
		}*/

		[Test]
		public void TestILPinning()
		{
			string s = "foo";
			Pointer<char> ptr = s;

			ObjectPinner.InvokeWhilePinned(s, delegate
			{
				Assertion.AssertPinning(ref s);
				Assertion.AssertElements(ptr, s);
			});
		}




	}

}