#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using RazorSharp;
using RazorSharp.Pointers;
using RazorSharp.Runtime;
using RazorSharp.Runtime.CLRTypes.HeapObjects;

#endregion

namespace Test.Testing
{

	internal static unsafe class RAssert
	{
		/// <summary>
		/// Asserts the ExPointer points to the proper array data.
		/// </summary>
		internal static void Elements<T>(ExPointer<T> ptr, IEnumerable<T> enumer)
		{
			var enumerator = enumer.GetEnumerator();
			while (enumerator.MoveNext()) {
				Assert.That(enumerator.Current, Is.EqualTo(ptr.Value));
				ptr++;
			}
		}

		private const int MaxPasses  = 1000;
		private const int MaxObjects = 9000;

		/// <summary>
		/// Asserts that the heap and stack pointers of a reference type don't change at all
		/// during GC compaction. This test will pass if the parameter is pinned.
		/// </summary>
		internal static void Pinning<T>(ref T t) where T : class
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
		/// Asserts that an ExPointer points to the correct object address during GC pressure
		/// </summary>
		internal static void Pressure<TPointer, TValue>(ExPointer<TPointer> ptr, ref TValue t)
		{
			int passes = 0;
			while (passes++ < MaxPasses) {
				object[] oArr = new object[MaxObjects];
				for (int i = 0; i < oArr.Length; i++) {
					oArr[i] = new object();
				}


				Assert.That(ptr.Value, Is.EqualTo(t));
				Assert.That(ptr.Address, Is.EqualTo(Unsafe.AddressOf(ref t)));
			}
		}

		internal static void HeapObject<T>(ref T t, HeapObject** h) where T : class
		{
			Debug.Assert((**h).Header == Runtime.ReadObjHeader(ref t));
			Debug.Assert((**h).MethodTable == Runtime.ReadMethodTable(ref t));
		}

		internal static void ArrayObject<T>(ref T[] arr, ArrayObject** ao)
		{
			Debug.Assert((**ao).Length == arr.Length);
			Debug.Assert((**ao).Header == Runtime.ReadObjHeader(ref arr));
			Debug.Assert((**ao).MethodTable == Runtime.ReadMethodTable(ref arr));

			//Debug.Assert((**ao).Handle.Value == typeof(T).TypeHandle.Value);
			/*if ((**ao).Handle.Value != IntPtr.Zero) {
				Debug.Assert((**ao).Handle.Value == typeof(T).TypeHandle.Value);
			}*/
		}

		internal static void StringObject(ref string s, StringObject** strObj)
		{
			Debug.Assert((**strObj).MethodTable == Runtime.ReadMethodTable(ref s));
			Debug.Assert((**strObj).Header == Runtime.ReadObjHeader(ref s));
			Debug.Assert((**strObj).Length == s.Length);
			Debug.Assert((**strObj).FirstChar == s[0]);
		}

		internal static void Pressure<TPointer>(ExPointer<TPointer> ptr, ref string s)
		{
			int passes = 0;
			while (passes++ < MaxPasses) {
				object[] oArr = new object[MaxObjects];
				for (int i = 0; i < oArr.Length; i++) {
					oArr[i] = new object();
				}
			}
		}
	}

}