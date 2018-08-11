#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using RazorCommon;
using RazorCommon.Strings;
using RazorSharp;
using RazorSharp.Analysis;
using RazorSharp.Memory;
using RazorSharp.Pointers;
using RazorSharp.Utilities;
using Test.Testing;
using static RazorSharp.Unsafe;

#endregion

namespace Test
{

	#region

	using CSUnsafe = System.Runtime.CompilerServices.Unsafe;

	#endregion

	/**
	 * RazorSharp
	 *
	 * History:
	 * 	- RazorSharp (deci-common-c)
	 * 	- RazorSharpNeue
	 * 	- RazorCLR
	 * 	- RazorSharp
	 *
	 * Notes:
	 *  - 32-bit is not fully supported
	 *  - Most types are probably not thread-safe
	 *
	 * Goals:
	 *  - Provide identical functionality of ClrMD, SOS, and Reflection
	 * 	  but in a faster and more efficient way
	 */
	internal static unsafe class Program
	{


#if DEBUG
		static Program()
		{
			StandardOut.ModConsole();
			Debug.Assert(IntPtr.Size == 8);
			Debug.Assert(Environment.Is64BitProcess);
			Logger.Log(Flags.Info, "Architecture: x64");
			Logger.Log(Flags.Info, "Byte order: {0}", BitConverter.IsLittleEndian ? "Little Endian" : "Big Endian");
			Logger.Log(Flags.Info, "CLR {0}", Environment.Version);
		}
#endif


		public static void Main(string[] args)
		{
			Vector v = new Vector();
			Inspector<Vector>.Write(ref v, true, InspectorMode.All & ~InspectorMode.MethodDescs);

			Clazz c = new Clazz();
			RefInspector<Clazz>.Write(ref c, true, InspectorMode.All & ~InspectorMode.MethodDescs);

			string s = "foo";
			RefInspector<string>.Write(ref s, false, InspectorMode.All & ~InspectorMode.MethodDescs);

			int[] arr = {1, 2, 3};
			RefInspector<int[]>.Write(ref arr, true, InspectorMode.All & ~InspectorMode.MethodDescs);

			string[] ptrArr = new[] {"foo"};
			RefInspector<string[]>.Write(ref ptrArr, true);

			//BenchmarkRunner.Run<LayoutBenchmarking>();


//			Console.ReadLine();
		}


		private class Clazz
		{
			private byte x;

			private long    l;
			private decimal d;

		}

		private struct Vector
		{
			private float _x;
			private float _y;
			private byte  b;


			public float X {
				get => _x;
				set => _x = value;
			}

			public float Y {
				get => _y;
				set => _y = value;
			}

			public Vector(float x, float y)
			{
				_x = x;
				_y = y;
				Debug.Assert(Memory.IsOnStack(ref _x));
				Debug.Assert(Memory.IsOnStack(ref _y));
				Debug.Assert(Memory.IsOnStack(ref x));
				Debug.Assert(Memory.IsOnStack(ref y));
				b = 0;
			}

			public override string ToString()
			{
				return String.Format("x = {0}, y = {1}", _x, _y);
			}
		}


		private static void DisplayTypes()
		{
			List<int> ls = new List<int>();
			RefInspector<List<int>>.Write(ref ls, false, InspectorMode.Address | InspectorMode.Internal);


			var s = "foo";
			RefInspector<string>.Write(ref s, false, InspectorMode.Address | InspectorMode.Internal);


			var d = new Dummy();
			RefInspector<Dummy>.Write(ref d, !false, InspectorMode.Address | InspectorMode.Internal);


			var parr = new string[5];
			RefInspector<string[]>.Write(ref parr, false, InspectorMode.Address | InspectorMode.Internal);


			var arr = new int[5];
			RefInspector<int[]>.Write(ref arr, false, InspectorMode.Address | InspectorMode.Internal);
		}

		private static void TableMethods<T>()
		{
			var table = new ConsoleTable("Function", "MethodDesc", "Name", "Virtual");
			foreach (var v in typeof(T).GetMethods(BindingFlags.Instance | BindingFlags.Public |
			                                       BindingFlags.NonPublic)) {
				table.AddRow(Hex.ToHex(v.MethodHandle.GetFunctionPointer()), Hex.ToHex(v.MethodHandle.Value),
					v.Name, v.IsVirtual ? StringUtils.Check : StringUtils.BallotX);
			}

			Console.WriteLine(table.ToMarkDownString());
		}

		private static void SetChar(this string str, int i, char c)
		{
			Pointer<char> lpChar = AddressOfHeap(ref str, OffsetType.StringData);
			lpChar[i] = c;
		}

		private static void RandomInit(AllocExPointer<string> ptr)
		{
			for (int i = 0; i < ptr.Count; i++) {
				ptr[i] = StringUtils.Random(10);
			}
		}

		/**
		 * Dependencies:
		 *
		 * RazorSharp:
		 *  - RazorCommon
		 * 	- CompilerServices.Unsafe
		 *  - RazorInvoke
		 *  - Fody
		 *  - MethodTimer Fody
		 *  - ObjectLayoutInspector
		 *
		 * Test:
		 *  - RazorCommon
		 *  - CompilerServices.Unsafe
		 * 	- NUnit
		 *  - BenchmarkDotNet
		 *  - Fody
		 *  - MethodTimer Fody
		 *  - ObjectLayoutInspector
		 */

		/**
		 * Class this ptr:
		 *
		 * public IntPtr __this {
		 *		get {
		 *			var v = this;
		 *			var hThis = Unsafe.AddressOfHeap(ref v);
		 *			return hThis;
		 *		}
		 *	}
		 *
		 *
		 * Struct this ptr:
		 *
		 * public IntPtr __this {
		 *		get => Unsafe.AddressOf(ref this);
		 * }
		 */

	}

}