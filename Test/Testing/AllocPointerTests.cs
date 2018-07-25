using System;
using NUnit.Framework;
using NUnit.Framework.Internal;
using RazorCommon.Strings;
using RazorSharp;
using RazorSharp.Pointers;

namespace Test.Testing
{
	[TestFixture]
	internal class AllocPointerTests
	{
		[Test]
		public void Test()
		{
			var alloc = new AllocPointer<string>(5);

			Assert.That(alloc.IsAllocated, Is.EqualTo(true));
			Assert.That(alloc.ElementSize, Is.EqualTo(Unsafe.SizeOf<string>()));
			Assert.That(alloc.IsNull, Is.EqualTo(false));
			Assert.That(alloc.Count, Is.EqualTo(5));
			Assert.That(alloc.IsDecayed, Is.EqualTo(false));


			// Init
			for (int i = 0; i < alloc.Count; i++) {
				alloc[i] = StringUtils.Random(i);
			}

		}


	}

}