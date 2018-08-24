#region

using System;
using System.Diagnostics;
using NUnit.Framework;
using RazorSharp;
using RazorSharp.Pointers.Ex;
using RazorSharp.Utilities;

#endregion

namespace Test.Testing.Tests
{

	[TestFixture]
	internal class AllocPointerTests
	{
		[Test]
		public void Test()
		{
			AllocExPointer<string> alloc = new AllocExPointer<string>(5)
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

			// Bounds checking

			RazorContract.AssertThrows<Exception>(delegate
			{
				AllocExPointer<string> p = new AllocExPointer<string>(5);
				p += p.Count + 1;
			});

			RazorContract.AssertThrows<Exception>(delegate
			{
				AllocExPointer<string> p = new AllocExPointer<string>(5);
				p -= p.Count + 1;
			});

			RazorContract.AssertThrows<Exception>(delegate
			{
				string x = alloc[-1];
			});

			RazorContract.AssertThrows<Exception>(delegate
			{
				string x = alloc[alloc.Count];
			});

			RazorContract.AssertThrows<Exception>(delegate
			{
				alloc++;
				string x = alloc[-2];
			});

			RazorContract.AssertNoThrows<Exception>(delegate
			{
				alloc--;
				string x = alloc[0];
			});

			alloc.Dispose();

			Assert.That(alloc.IsAllocated, Is.EqualTo(false));
			Assert.That(alloc.Address, Is.EqualTo(IntPtr.Zero));
			Assert.That(alloc.Value, Is.EqualTo(default));
			Assert.That(alloc[0], Is.EqualTo(default));

			alloc.MoveToStart();

			for (int i = 0; i < alloc.Count; i++) {
				for (int j = alloc.Start; j <= alloc.End; j++) Debug.Assert(alloc.IndexInBounds(j));

				alloc++;
			}

			for (int i = 0; i < alloc.Count; i++) {
				for (int j = alloc.Start; j <= alloc.End; j++) Debug.Assert(alloc.IndexInBounds(j));

				alloc--;
			}

			alloc.Address = alloc.LastElement;
			Debug.Assert(alloc.Value == alloc[alloc.End]);


			alloc.Address = alloc.FirstElement;
			Debug.Assert(alloc.Value == alloc[alloc.Start]);

			Debug.Assert(!alloc.IndexInBounds(-1));
			Debug.Assert(!alloc.IndexInBounds(5));

			const int zero = 123;
			const int end  = 0xFF;

			alloc.Dispose();


			AllocExPointer<int> allocI = new AllocExPointer<int>(5)
			{
				[0] = zero,
				[4] = end
			};

			for (int i = allocI.Start; i <= allocI.End; i++) {
				Debug.Assert(end == allocI[allocI.End]);
				Debug.Assert(zero == allocI[allocI.Start]);
				Debug.Assert(allocI.IndexOf(end) == allocI.End);
				Debug.Assert(allocI.IndexOf(zero) == allocI.Start);
				allocI++;
			}

			allocI.Address = allocI.LastElement;

			Debug.Assert(allocI.AddressInBounds(allocI.Address));

			Debug.Assert(!allocI.AddressInBounds(allocI.Address + 1));
		}


	}

}