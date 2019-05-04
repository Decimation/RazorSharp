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
using JetBrains.Annotations;
using RazorSharp.Analysis;
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


		[ClrSymNamespace]
		struct Globals
		{
			[SymField(Options = SymImportOptions.FullyQualified, FieldOptions = SymFieldOptions.LoadDirect)]
			public Pointer<_MethodTable> g_pStringClass;
		}

		[StructLayout(LayoutKind.Sequential)]
		struct _MethodTable
		{
			public int m_dwFlags;
			public int BaseSize;

			public override string ToString()
			{
				return Inspect.ValuesString(this);
			}
		}


		public static void Main(string[] args)
		{
			object o = 123D;

			Console.WriteLine(Unsafe.HeapSize(o));
			
			Debug.Assert(Unsafe.HeapSize(o)==Unsafe.SizeOfAuto(o, SizeOfOptions.Heap));
			Debug.Assert(Unsafe.SizeOf<object>()==Unsafe.SizeOfAuto(o, SizeOfOptions.Intrinsic));
			Debug.Assert(Unsafe.BaseFieldsSize<object>()==Unsafe.SizeOfAuto<object>(default, SizeOfOptions.BaseFields));
			Debug.Assert(Unsafe.BaseFieldsSize<object>(o)==Unsafe.SizeOfAuto<object>(o, SizeOfOptions.BaseFields));
			Debug.Assert(Unsafe.BaseInstanceSize<object>()==Unsafe.SizeOfAuto(o, SizeOfOptions.BaseInstance));
		}
	}
}