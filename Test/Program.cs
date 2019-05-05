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

		const string dll = @"C:\Users\Deci\CLionProjects\NativeLib64\cmake-build-debug\NativeLib64.dll";
		const string pdb = @"C:\Users\Deci\CLionProjects\NativeLib64\cmake-build-debug\NativeLib64.pdb";

		[SymNamespace(pdb, dll)]
		class Structure
		{
			[SymField(SymImportOptions.FullyQualified)]
			public int g_int32;

			[SymCall(SymImportOptions.IgnoreEnclosingNamespace)]
			public void hello()
			{
				Console.WriteLine("Orig");
			}
		}

		[DllImport(dll)]
		static extern void RunFunc();


		static void test()
		{
			var struc = new Structure();

			struc.hello();

			struc = Symload.Load(struc);


			struc.hello();


			Symload.Reload(ref struc);

			Console.WriteLine(struc.g_int32);


			Symload.Unload(ref struc);

			Console.WriteLine("Unload");
			Console.WriteLine(struc.g_int32);


			struc.hello();
		}

		private static char get_Chars(string s, int i)
		{
			return default;
		}

		private static long ADDR  = Runtime.GetClrSymAddress("JIT_GetRuntimeType").ToInt64();
		private static long ADDR2 = Runtime.GetClrSymAddress("JIT_GetRuntimeType").ToInt64();

		public static Type Calli(long md)
		{
			IL.Emit.Ldarg_0();
			IL.Emit.Ldsfld(new FieldRef(typeof(Program), nameof(ADDR)));
			IL.Emit.Conv_I();
			IL.Emit.Calli(new StandAloneMethodSig(CallingConventions.Standard,
			                                      new TypeRef(typeof(Type)),
			                                      new TypeRef(typeof(void*))));
			return IL.Return<Type>();
		}

		public static Type __Calli(long md)
		{
			return null;
		}


		private static void GenIL(byte[] il)
		{
			Type cls         = typeof(Program);
			int  methodToken = cls.GetAnyMethod("__Calli").MetadataToken;
			int  methodSize  = il.Length;
			int  flags       = 0;

			fixed (byte* p = il) {
				MethodRental.SwapMethodBody(cls, methodToken, (IntPtr) p, methodSize, flags);
			}
		}

		[HandleProcessCorruptedStateExceptions]
		public static void Main(string[] args)
		{
			// SetMethodEntryPoint

			var s = new Structure();
			
            s.hello();
            s = Symload.Load(s);
            s.hello();
		}
	}
}