#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using NUnit.Framework;
using RazorSharp;
using RazorSharp.Analysis;
using RazorSharp.CLR;
using RazorSharp.CLR.Structures.HeapObjects;
using RazorSharp.Common;
using RazorSharp.Pointers;
using Test.Testing.Types;

#endregion

namespace Test.Testing
{
	internal static unsafe class TestingUtil
	{
		private const int MaxPasses  = 1000;
		private const int MaxObjects = 9000;

		/// <summary>
		///     Asserts that the heap and stack pointers of a reference type don't change at all
		///     during GC compaction. This test will pass if the parameter is pinned.
		/// </summary>
		internal static void Pinning<T>(ref T t) where T : class
		{
			(IntPtr stackPtr, IntPtr heap) mem = (Unsafe.AddressOf(ref t).Address, Unsafe.AddressOfHeap(ref t).Address);


			int passes = 0;
			while (passes++ < MaxPasses) {
				var oArr = new object[MaxObjects];
				for (int i = 0; i < oArr.Length; i++)
					oArr[i] = new object();

				// pass when reference is pinned
				Assert.That(mem.stackPtr, Is.EqualTo(Unsafe.AddressOf(ref t)));
				Assert.That(mem.heap, Is.EqualTo(Unsafe.AddressOfHeap(ref t)));
			}
		}

		internal static void Pinning2<T>(ref T t) where T : class
		{
			(IntPtr stackPtr, IntPtr heap) mem = (Unsafe.AddressOf(ref t).Address, Unsafe.AddressOfHeap(ref t).Address);


			int passes = 0;
			while (passes++ < MaxPasses) {
				var oArr                                      = new object[MaxObjects];
				for (int i = 0; i < oArr.Length; i++) oArr[i] = new object();

				// pass when reference is pinned
				if (mem.stackPtr != Unsafe.AddressOf(ref t)) {
					//Console.WriteLine("Stack: {0} != {1}", Hex.ToHex(mem.stackPtr), Hex.ToHex(Unsafe.AddressOf(ref t)));
					//break;
				}

				if (mem.heap != Unsafe.AddressOfHeap(ref t)) {
					Console.WriteLine("Heap: {0} != {1}", Hex.ToHex(mem.heap),
						Hex.ToHex(Unsafe.AddressOfHeap(ref t).Address));
					break;
				}
			}
		}

		internal static void CreateGCPressure()
		{
			int passes = 0;
			while (passes++ < MaxPasses) {
				var oArr = new object[MaxObjects];
				for (int i = 0; i < oArr.Length; i++)
					oArr[i] = new object();
			}
		}


		internal static void Pressure<TPointer, TValue>(Pointer<TPointer> ptr, ref TValue t)
		{
			int passes = 0;
			while (passes++ < MaxPasses) {
				var oArr = new object[MaxObjects];
				for (int i = 0; i < oArr.Length; i++)
					oArr[i] = new object();


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

		internal static void Pressure<TPointer>(Pointer<TPointer> ptr, ref string s)
		{
			int passes = 0;
			while (passes++ < MaxPasses) {
				var oArr = new object[MaxObjects];
				for (int i = 0; i < oArr.Length; i++)
					oArr[i] = new object();
			}
		}

		internal static void DumpArray<T>(ref T[] arr) where T : class
		{
			Pointer<long> lpRg = Unsafe.AddressOfHeap(ref arr, OffsetType.ArrayData).Address;
			for (int i = 0; i < arr.Length; i++) {
				Console.WriteLine("{0} : {1}", Hex.ToHex(lpRg.ReadAny<long>()), lpRg.ReadAny<T>());
				lpRg++;
			}
		}

		public static void TestTypes()
		{
			/**
			 * string
			 *
			 * Generic:		no
			 * Type:		reference
			 */
			string s = "foo";
			InspectorHelper.Inspect(ref s);

			/**
			 * int[]
			 *
			 * Generic:		no
			 * Type:		reference
			 */
			int[] arr = {1, 2, 3};
			InspectorHelper.Inspect(ref arr, true);

			/**
			 * string[]
			 *
			 * Generic:		no
			 * Type:		reference
			 */
			string[] ptrArr = {"foo", "bar", "desu"};
			InspectorHelper.Inspect(ref ptrArr);

			/**
			 * int
			 *
			 * Generic:		no
			 * Type:		value
			 */
			int i = 0xFF;
			InspectorHelper.InspectVal(ref i);

			/**
			 * Dummy
			 *
			 * Generic:		no
			 * Type:		reference
			 */
			var d = new Dummy();
			InspectorHelper.Inspect(ref d);

			/**
			 * List<int>
			 *
			 * Generic:		yes
			 * Type:		reference
			 */
			var ls = new List<int>();
			InspectorHelper.Inspect(ref ls);

			/**
			 * Point
			 *
			 * Generic:		no
			 * Type:		value
			 *
			 */
			var pt = new Point();
			InspectorHelper.InspectVal(ref pt);

			/**
			 * char
			 */
			char c = '\0';
			InspectorHelper.InspectVal(ref c);
		}

		internal static void TableMethods<T>()
		{
			var table = new ConsoleTable("Function", "MethodDesc", "Name", "Virtual");
			foreach (var v in typeof(T).GetMethods(BindingFlags.Instance | BindingFlags.Public |
			                                       BindingFlags.NonPublic))
				table.AddRow(Hex.ToHex(v.MethodHandle.GetFunctionPointer()), Hex.ToHex(v.MethodHandle.Value),
					v.Name, v.IsVirtual.Prettify());

			Console.WriteLine(table.ToMarkDownString());
		}
	}
}