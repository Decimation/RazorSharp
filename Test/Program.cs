using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Threading;
using BenchmarkDotNet.Running;
using MethodTimer;
using NUnit.Framework;
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
		 * RazorSharp:
		 *  - RazorCommon
		 * 	- CompilerServices.Unsafe
		 *  - RazorInvoke
		 *  - Fody
		 *  - MethodTimer Fody
		 *
		 * Test:
		 *  - RazorCommon
		 *  - CompilerServices.Unsafe
		 * 	- NUnit
		 *  - BenchmarkDotNet
		 *  - Fody
		 *  - MethodTimer Fody
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
			Dummy d = new Dummy();
			RefInspector<Dummy>.Write(ref d);

			Console.WriteLine(
				Hex.ToHex(PointerUtils.Offset<byte>(Runtime.MethodTableOf<Dummy>(), sizeof(MethodTable))));
			Console.WriteLine();

			Console.WriteLine("FieldDesc:");
			foreach (var v in Runtime.GetFieldDescs<Dummy>(BindingFlags.Instance | BindingFlags.NonPublic |
			                                               BindingFlags.Static | BindingFlags.Public)) {
				Console.WriteLine(Hex.ToHex(v.Address));
			}

			Console.WriteLine();

			Console.WriteLine("MethodDesc:");
			foreach (var v in Runtime.GetMethodDescs<Dummy>(BindingFlags.Instance | BindingFlags.NonPublic |
			                                                BindingFlags.Static | BindingFlags.Public)) {
				Console.WriteLine(Hex.ToHex(v.Address));
			}

			Console.WriteLine();

			var hMethod = typeof(Dummy).GetMethod("DoSomething", BindingFlags.Instance | BindingFlags.Public)
				.MethodHandle;
			var md1 = (MethodDesc*) hMethod.Value;

			//Console.WriteLine(Hex.ToHex(hMethod.Value));
			//Console.WriteLine(Hex.ToHex(hMethod.GetFunctionPointer()));
			//Console.WriteLine(*md1);



		}

		private static void Stat<T>()
		{

		}


		private static void ManualTable<T>(AllocPointer<T> alloc)
		{
			bool refType = !typeof(T).IsValueType;

			ConsoleTable table =
				refType
					? new ConsoleTable("Index", "Address", "Value", "Heap pointer")
					: new ConsoleTable("Index", "Address", "Value");

			for (int i = alloc.Start; i <= alloc.End; i++) {
				var addr = PointerUtils.Offset<T>(alloc.Address, i);

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


	}

}