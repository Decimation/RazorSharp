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
using RazorCommon;
using RazorCommon.Diagnostics;
using RazorCommon.Extensions;
using RazorCommon.Strings;
using RazorCommon.Utilities;
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

#endregion


namespace Test
{
	#region

	using DWORD = UInt32;
	using Ptr = Pointer<byte>;

	#endregion


	public static unsafe class Program
	{
		// Common library: RazorCommon
		// Testing library: Sandbox


		private static void __Compile(Type t, string n)
		{
			RuntimeHelpers.PrepareMethod(t.GetAnyMethod(n).MethodHandle);
		}

		static string[] GetPathValues()
		{
			var raw = Environment.GetEnvironmentVariable("path", EnvironmentVariableTarget.Machine);
			Conditions.NotNull(raw, nameof(raw));
			return raw.Split(';');
		}

		const string pdb2 = @"C:\Users\Deci\CLionProjects\NativeSharp\cmake-build-debug\NativeSharp.pdb";
		const string dll  = @"C:\Users\Deci\CLionProjects\NativeSharp\cmake-build-debug\NativeSharp.dll";

		/*[SymNamespace(pdb2, "NativeSharp.dll")]
		private struct MyStruct
		{
			[SymField(UseMemberNameOnly = true)]
			public int g_int;

			[Symcall(UseMemberNameOnly = true)]
			public void hello() { }
		}*/


		// todo: symbol address/offset difference between pdb (PdbFile) and kernel (DbgHelp)

		// todo: massive overhaul and refactoring

		// todo: DIA instead of dbghelp?

		struct NativeLinkedList
		{
			private NativeNode* m_head;

			public static NativeLinkedList Alloc()
			{
				var list = new NativeLinkedList
				{
					m_head = (NativeNode*) Mem.Alloc<NativeNode>()
				};
				return list;
			}
		}

		struct NativeNode { }

		private static Pointer<char> sz;

		static void Nullptr<T>(Pointer<Pointer<T>> p)
		{
			p.Reference = sz.Cast<T>();
		}

		

		private static string nullptr_t = "Nullptr_t";

		public static void Main(string[] args)
		{
			ModuleInitializer.GlobalSetup();

			int  v = 256;
			int* i = &v;

			Pointer<char> p = null;
			string        s = "foo";
			sz = Unsafe.AddressOfHeap(s, OffsetOptions.STRING_DATA);
			var p2 = Unsafe.AddressOf(ref p);
			Nullptr(p2);
			Console.WriteLine("{0:O} {0:P}", p2.Reference);

			const ulong ul = 1UL;
			Console.WriteLine(ul);

			
			Console.WriteLine(Unsafe.IsNil<string>(null));
			
			ModuleInitializer.GlobalClose();
		}
	}
}