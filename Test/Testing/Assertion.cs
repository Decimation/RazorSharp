using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using RazorSharp;
using RazorSharp.Pointers;

namespace Test.Testing
{

	internal static class Assertion
	{
		/// <summary>
		/// Asserts the Pointer points to the proper array data.
		/// </summary>
		internal static void AssertElements<T>(Pointer<T> ptr, IEnumerable<T> enumer)
		{
			var enumerator = enumer.GetEnumerator();
			while (enumerator.MoveNext()) {
				//Console.Clear();
				//Console.Write("{0:T}",ptr);
				Assert.That(enumerator.Current, Is.EqualTo(ptr.Value));
				ptr++;
				//Thread.Sleep(1000);
			}

			if (typeof(ArrayPointer<T>) == ptr.GetType()) {
				((ArrayPointer<T>)ptr).MoveToStart();

			}


		}

		private const int MaxPasses  = 1000;
		private const int MaxObjects = 9000;

		/// <summary>
		/// Asserts that the heap and stack pointers of a reference type don't change at all
		/// during GC compaction. This test will pass if the parameter is pinned.
		/// </summary>
		internal static void AssertPinning<T>(ref T t) where T : class
		{
			(IntPtr stackPtr, IntPtr heap) mem = (Unsafe.AddressOf(ref t), Unsafe.AddressOfHeap(ref t));


			int passes = 0;
			while (passes++ < MaxPasses) {

				object[] oArr = new object[MaxObjects];
				for (int i = 0; i < oArr.Length; i++) {
					oArr[i] = new object();
				}

				// pass when reference is pinned
				Assert.That(mem.stackPtr, Is.EqualTo(Unsafe.AddressOf(ref t)));
				Assert.That(mem.heap, Is.EqualTo(Unsafe.AddressOfHeap(ref t)));
			}
		}

		/// <summary>
		/// Asserts that a Pointer points to the correct object address during GC pressure
		/// </summary>
		internal static void AssertPressure<TPointer, TValue>(Pointer<TPointer> ptr, ref TValue t)
		{
			int passes = 0;
			while (passes++ < MaxPasses) {

				object[] oArr = new object[MaxObjects];
				for (int i = 0; i < oArr.Length; i++) {
					oArr[i] = new object();
				}

				if (ptr.IsDecayed) {

				}
				else {
					Assert.That(ptr.Value, Is.EqualTo(t));
					Assert.That(ptr.Address, Is.EqualTo(Unsafe.AddressOf(ref t)));
				}

			}
		}
	}

}