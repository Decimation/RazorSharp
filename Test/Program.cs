#region

using System;
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
using RazorSharp.Memory.Extern.Symbols;
using RazorSharp.Memory.Extern.Symbols.Attributes;
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


		// todo: symbol address/offset difference between pdb (PdbFile) and kernel (DbgHelp)

		// todo: massive overhaul and refactoring

		// todo: DIA instead of dbghelp?

		private static void Test()
		{
			var s = new Structure();

			s.hello();

			s = Symload.Load(s);
			s.hello();


			Symload.Reload(ref s);

			Console.WriteLine(s.g_int32);
			Symload.Unload(ref s);
			Console.WriteLine(s.g_int32);

			Console.WriteLine(Unsafe.SizeOf<int>());
		}

		[HandleProcessCorruptedStateExceptions]
		public static void Main(string[] args)
		{
			SymbolManager.CurrentImage = new FileInfo(Structure.PDB);


			var mod=Modules.LoadModule(Structure.DLL);
			var mi = new ModuleInfo(new FileInfo(Structure.PDB), mod);
			var sz = mi.GetSymAddress("sz").ReadPointer<byte>();
			Conditions.Assert(sz.ReadString(StringTypes.ANSI)=="Hello, World");


			Pointer<EEClass> ee = typeof(string).GetMethodTable()
			                                    .Reference
			                                    .EEClass;

			Console.WriteLine(ee);
			Console.WriteLine(ee.Reference.FieldDescList.ToString("O"));
		}
	}
}