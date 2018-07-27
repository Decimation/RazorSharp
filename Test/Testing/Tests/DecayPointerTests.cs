using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using RazorCommon.Strings;
using RazorSharp;
using RazorSharp.Pointers;

namespace Test.Testing.Tests
{

	[TestFixture]
	internal class DecayPointerTests
	{
		[Test]
		public void TestArray()
		{
			int[]             arr       = {1, 2, 3, 4, 5};
			DecayPointer<int> arrPtrInt = arr;
			AssertDecayPointer(arrPtrInt, arr);

			long[]             longArr = {1, 2, 3, 4, 5};
			DecayPointer<long> longPtr = longArr;
			AssertDecayPointer(longPtr, longArr);

			string[]             strArr    = {StringUtils.Random(5), StringUtils.Random(10), "foo", "anime", ""};
			DecayPointer<string> strArrPtr = strArr;
			AssertDecayPointer(strArrPtr, strArr);
		}



		private static void AssertDecayPointer<TElement>(DecayPointer<TElement> ptr, IList<TElement> arr)
		{
			Assert.That(ptr.Count, Is.EqualTo(arr.Count));
			Assert.That(ptr.IsDecayed, Is.EqualTo(true));
			Assert.That(ptr.IsNull, Is.EqualTo(false));

			TestingAssertion.AssertElements(ptr, arr);
			ptr.MoveToStart();


			for (int i = 0; i < arr.Count; i++) {
				Debug.Assert(ptr[i].Equals(arr[i]));
			}

			for (int i = 0; i < arr.Count; i++) {
				Debug.Assert(ptr[0].Equals(arr[i]));
				Debug.Assert(ptr.Value.Equals(arr[i]));
				ptr++;
			}



			for (int i = arr.Count - 1; i >= 0; i--) {
				Assert.That(ptr.Value, Is.EqualTo(arr[i]));
				ptr--;
			}

			Assert.That(ptr.Value, Is.EqualTo(arr[0]));
			Assert.That(ptr[0], Is.EqualTo(arr[0]));

			//Debug.Assert(ptr.ToArray().SequenceEqual(arr));
			TestingAssertion.AssertElements(ptr, arr);
		}


		[Test]
		public void TestString()
		{
			string s = "anime";

			DecayPointer<char> ptr = s;

			Assert.That(ptr.FirstElement, Is.EqualTo(Unsafe.AddressOfHeap(ref s, OffsetType.StringData)));


			// Test indexing
			for (int i = 0; i < s.Length; i++) {
				Assert.That(ptr[i], Is.EqualTo(s[i]));
			}

			// Test incrementing
			for (int i = 0; i < s.Length; i++, ptr++) {
				Assert.That(ptr.Value, Is.EqualTo(s[i]));
				Assert.That(ptr[0], Is.EqualTo(s[i]));
			}

			// Test decrementing
			for (int i = s.Length - 1; i >= 0; i--) {
				Assert.That(ptr.Value, Is.EqualTo(s[i]));
				ptr--;
			}

			Assert.That(ptr.Value, Is.EqualTo(s[0]));
			Assert.That(ptr[0], Is.EqualTo(s[0]));

			//s += " bar";
			Assert.That(ptr.Address, Is.EqualTo(Unsafe.AddressOfHeap(ref s, OffsetType.StringData)));
			Assert.That(ptr.Count, Is.EqualTo(s.Length));

			//Debug.Assert(ptr.ToArray().SequenceEqual(s));
		}
	}

}