using System;
using System.Diagnostics;
using NUnit.Framework;
using RazorCommon.Strings;
using RazorSharp;
using RazorSharp.Pointers;
using RazorSharp.Utilities;

namespace Test.Testing.Tests
{
	[TestFixture]
	internal class AllocPointerTests
	{
		[Test]
		public void Test()
		{
			var alloc = new AllocPointer<string>(5)
			{
				[0] = "g",
				[1] = "anime",
				[2] = "waifu",
				[3] = "animanga",
				[4] = "nyaa~"
			};

			Assert.That(alloc.Start, Is.EqualTo(0));
			Assert.That(alloc.End, Is.EqualTo(4));

			Assert.That(alloc.IsAllocated, Is.EqualTo(true));
			Assert.That(alloc.ElementSize, Is.EqualTo(Unsafe.SizeOf<string>()));
			Assert.That(alloc.IsNull, Is.EqualTo(false));
			Assert.That(alloc.Count, Is.EqualTo(5));
			Assert.That(alloc.IsDecayed, Is.EqualTo(false));

			// Bounds checking

			Assertion.AssertThrows<Exception>(delegate
			{
				var p = new AllocPointer<string>(5);
				p += p.Count + 1;
			});

			Assertion.AssertThrows<Exception>(delegate
			{
				var p = new AllocPointer<string>(5);
				p -= p.Count + 1;
			});

			Assertion.AssertThrows<Exception>(delegate
			{
				var x = alloc[-1];

			});

			Assertion.AssertThrows<Exception>(delegate
			{
				var x = alloc[alloc.Count];
			});

			Assertion.AssertThrows<Exception>(delegate
			{
				alloc++;
				var x = alloc[-2];
			});

			Assertion.AssertNoThrows<Exception>(delegate
			{
				alloc--;
				var x = alloc[0];
			});

			alloc.Dispose();

			Assert.That(alloc.IsAllocated, Is.EqualTo(false));
			Assert.That(alloc.Address, Is.EqualTo(IntPtr.Zero));
			Assert.That(alloc.Value, Is.EqualTo(default));
			Assert.That(alloc[0], Is.EqualTo(default));




		}


	}

}