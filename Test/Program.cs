#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Environments;
using RazorCommon;
using RazorCommon.Strings;
using RazorSharp;
using RazorSharp.Analysis;
using RazorSharp.Pointers;
using RazorSharp.Utilities;
using Test.Testing;
using static RazorSharp.Unsafe;
using Runtime = RazorSharp.Runtime.Runtime;

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
			Structure structure = new Structure();

			RefInspector<Structure>.Write(ref structure);

			foreach (var v in Runtime.GetFieldDescs<Structure>()) {
				Console.WriteLine(v->ToString());
			}
		}

		class Structure
		{
			private            string PrivateString;
			public             string PublicString;
			protected          string ProtectedString;
			internal           string InternalString;
			protected internal string ProtectedInternalString;
			private protected  string PrivateProtectedString;
		}


		private static readonly Dummy d = new Dummy();


		private static void DisplayTypes<T>(ref T t) where T : class
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