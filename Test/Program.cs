using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
using Test.Testing;
using Test.Testing.Benchmarking;
using Unsafe = RazorSharp.Unsafe;
using static RazorSharp.Utilities.Assertion;
using Assertion = RazorSharp.Utilities.Assertion;

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
			byte* stackMem = stackalloc byte[Unsafe.BaseInstanceSize<Dummy>()];
			var stackDummy = new StackAllocated<Dummy>(stackMem);
			Console.WriteLine(stackDummy);
			
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

		


		// todo
		private static T NewStackAlloc<T>(byte* stackPtr) where T : class
		{
			T t = Activator.CreateInstance<T>();


			Console.WriteLine(Unsafe.HeapSize(ref t));
			var allocSize = Unsafe.BaseInstanceSize<T>();
			var heapMem   = Unsafe.MemoryOf(ref t);

			for (int i = 0; i < allocSize; i++) {
				stackPtr[i] = heapMem[i];
			}

			// Move over ObjHeader
			stackPtr += IntPtr.Size;

			Unsafe.WriteReference(ref t, stackPtr);
			return t;
		}

		private static void ReStackAlloc<T>(byte* stackPtr, ref T t) where T : class
		{
			var allocSize = Unsafe.HeapSize(ref t);
			var heapAddr  = (byte*) Unsafe.AddressOfHeap(ref t) - IntPtr.Size;
			for (int i = 0; i < allocSize; i++) {
				stackPtr[i] = heapAddr[i];
			}

			// Move over ObjHeader
			stackPtr += IntPtr.Size;
			Unsafe.WriteReference(ref t, stackPtr);
		}


		//todo
		private static void ModuleInfo(IntPtr module)
		{
			long* addrPtr = (long*) module.ToPointer();

			var assembly                = addrPtr + 6;
			var typeDefToMethodTableMap = addrPtr + 48;
			var typeRefToMethodTableMap = typeDefToMethodTableMap + 9;
			var methodDefToDescMap      = typeRefToMethodTableMap + 9;
			var fieldDefToDescMap       = methodDefToDescMap + 9;

			var table = new ConsoleTable("Data", "Address");

			table.AddRow("Assembly", Hex.ToHex(*assembly));
			table.AddRow("TypeDefToMethodTableMap", Hex.ToHex(*typeDefToMethodTableMap));
			table.AddRow("TypeRefToMethodTableMap", Hex.ToHex(*typeRefToMethodTableMap));
			table.AddRow("MethodDefToDescMap", Hex.ToHex(*methodDefToDescMap));
			table.AddRow("FieldDefToDescMap", Hex.ToHex(*fieldDefToDescMap));
			Console.WriteLine(table.ToMarkDownString());
		}

	}

}