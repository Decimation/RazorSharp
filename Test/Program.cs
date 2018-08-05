using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using BenchmarkDotNet.Running;
using MethodTimer;
using NUnit.Framework;
using ObjectLayoutInspector;
using RazorCommon;
using RazorCommon.Strings;
using RazorSharp;
using RazorSharp.Analysis;
using RazorSharp.Experimental;
using RazorSharp.Memory;
using RazorSharp.Pointers;
using RazorSharp.Runtime;
using RazorSharp.Runtime.CLRTypes;
using RazorSharp.Runtime.CLRTypes.HeapObjects;
using RazorSharp.Utilities;
using RazorSharp.Virtual;
using Test.Testing;
using Test.Testing.Benchmarking;
using Unsafe = RazorSharp.Unsafe;
using static RazorSharp.Unsafe;
using static RazorSharp.Utilities.Assertion;
using Assertion = RazorSharp.Utilities.Assertion;
using Module = RazorSharp.Runtime.CLRTypes.Module;

namespace Test
{

	using CSUnsafe = System.Runtime.CompilerServices.Unsafe;

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
		public static void Main(string[] args)
		{
			string @string = "foo";
			RefInspector<string>.Write(ref @string,true);

			string[] ptrArr = new string[0];
			RefInspector<string[]>.Write(ref ptrArr, true);
		}

		private static void SetChar(this string str, int i, char c)
		{
			LitePointer<char> lpChar = AddressOfHeap(ref str, OffsetType.StringData);
			lpChar[i] = c;
		}

		private static void ManualTable<T>(AllocPointer<T> alloc)
		{
			bool refType = !typeof(T).IsValueType;

			ConsoleTable table =
				refType
					? new ConsoleTable("Index", "Address", "Value", "Heap pointer")
					: new ConsoleTable("Index", "Address", "Value");

			for (int i = alloc.Start; i <= alloc.End; i++) {
				IntPtr addr = PointerUtils.Offset<T>(alloc.Address, i);

				if (refType) {
					table.AddRow(i, Hex.ToHex(addr), alloc[i], Hex.ToHex(Marshal.ReadIntPtr(addr)));
				}
				else {
					table.AddRow(i, Hex.ToHex(addr), alloc[i]);
				}
			}


			Console.WriteLine(table.ToMarkDownString());
		}

		private static void RandomInit(AllocPointer<string> ptr)
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