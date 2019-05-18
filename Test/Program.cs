#region

using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using InlineIL;
using SimpleSharp;
using SimpleSharp.Diagnostics;
using SimpleSharp.Extensions;
using SimpleSharp.Strings;
using SimpleSharp.Utilities;
using RazorSharp;
using RazorSharp.CoreClr;
using RazorSharp.CoreClr.Structures;
using RazorSharp.CoreClr.Structures.ILMethods;
using RazorSharp.Memory;
using RazorSharp.Memory.Pointers;
using RazorSharp.Native;
using RazorSharp.Native.Symbols;
using RazorSharp.Native.Win32;
using RazorSharp.Utilities;
using CSUnsafe = System.Runtime.CompilerServices.Unsafe;
using Unsafe = RazorSharp.Memory.Unsafe;
using System.Net.Http;
using System.Runtime.ExceptionServices;
using JetBrains.Annotations;
using RazorSharp.Analysis;
using RazorSharp.CoreClr.Meta;
using RazorSharp.CoreClr.Structures.EE;
using RazorSharp.Import;

#endregion


namespace Test
{
	#region

	using DWORD = UInt32;
	using Ptr = Pointer<byte>;

	#endregion


	public static unsafe class Program
	{
		// Common library: SimpleSharp
		// Testing library: Sandbox


		// todo: maybe switch to SharpPdb

		// todo: massive overhaul and refactoring

		// todo: DIA instead of dbghelp?

		// todo: rewrite ToString methods

		private static void Test()
		{
			var s = new Structure();

			s.hello();

			s = Symload.Load(s);
			s.hello();

			Pointer<byte> p = s.g_szStr;
			Console.WriteLine("const char*: {0}", p.ReadCString());

			Pointer<byte> p2 = s.g_szWStr;
			Console.WriteLine("const wchar_t*: {0}", p2.ReadCString(StringTypes.UNI));

			Pointer<byte> p3 = s.g_sz16Str;
			Console.WriteLine("const char16_t*: {0}", p3.ReadCString(StringTypes.UNI));

			Pointer<byte> p4 = s.g_sz32Str;
			Console.WriteLine("const char32_t*: {0}", p4.ReadCString(StringTypes.CHAR32));

			Symload.Reload(ref s);

			Console.WriteLine(s.g_int32);
			Symload.Unload(ref s);
			Console.WriteLine(s.g_int32);


			Console.WriteLine(Unsafe.SizeOf<int>());
		}

		private static void Test<T>(T value)
		{
			var options = InspectOptions.Values | InspectOptions.FieldOffsets
			                                    | InspectOptions.Addresses
			                                    | InspectOptions.InternalStructures
			                                    | InspectOptions.MemoryOffsets
			                                    | InspectOptions.AuxiliaryInfo
			                                    | InspectOptions.ArrayOrString;

			var layout = Inspect.Layout<T>(InspectOptions.Types);
			layout.Options |= options;
			layout.Populate(ref value);
			Console.WriteLine(layout);
		}


		[HandleProcessCorruptedStateExceptions]
		public static void Main(string[] args)
		{
			var ptr = Marshal.AllocHGlobal(256);
			var ai  = new AddressInfo(ptr);
			Console.WriteLine(ai);
			Console.WriteLine(Mem.IsValid(ptr));

			int i = 255;
			Pointer<int> p = &i;
			Console.WriteLine(Kernel32.VirtualQuery(p.Address).IsReadable);

			foreach (var heap in HeapApi.GetProcessHeaps()) {
				Console.WriteLine(Hex.ToHex(heap));
			}
		}
	}
}