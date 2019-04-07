#region

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using RazorCommon;
using RazorCommon.Diagnostics;
using RazorCommon.Strings;
using RazorSharp;
using RazorSharp.Analysis;
using RazorSharp.CoreClr;
using RazorSharp.CoreClr.Structures;
using RazorSharp.CoreClr.Structures.EE;
using RazorSharp.Memory;
using RazorSharp.Memory.Calling.Symbols;
using RazorSharp.Memory.Calling.Symbols.Attributes;
using RazorSharp.Memory.Fixed;
using RazorSharp.Memory.Pointers;
using RazorSharp.Native;
using RazorSharp.Native.Symbols;
using RazorSharp.Native.Win32;
using RazorSharp.Utilities;
using CSUnsafe = System.Runtime.CompilerServices.Unsafe;
using Unsafe = RazorSharp.Memory.Unsafe;

#endregion


namespace Test
{
	#region

	using DWORD = UInt32;

	#endregion

	using Ptr = Pointer<byte>;

	public static unsafe class Program
	{
		// todo: replace native pointers* with Pointer<T> for consistency
		// todo: RazorSharp, ClrMD, Reflection, Cecil comparison

		// Common library: RazorCommon
		// Testing library: RazorSandbox

		[ClrSymcall(Symbol = "Object::GetSize", FullyQualified = true)]
		private static int Size(this object obj)
		{
			return Constants.INVALID_VALUE;
		}

		[DllImport("kernel32")]
		private static extern IntPtr GetCurrentProcess();

		[AttributeUsage(AttributeTargets.Method)]
		class Attr : Attribute
		{
			internal string Image  { get; }
			internal string Name   { get; }
			internal string Module { get; }

			public Attr(string img, string module, string name)
			{
				Image  = img;
				Name   = name;
				Module = module;
			}
		}


		class MyStruct
		{
			// #define IDS_CLASSLOAD_MISSINGMETHODRVA          0x1797

			// MethodTableBuilder::ValidateMethods()
			// https://github.com/dotnet/coreclr/blob/master/src/vm/methodtablebuilder.cpp
			// 4880: BuildMethodTableThrowException(IDS_CLASSLOAD_MISSINGMETHODRVA, it.Token());
//			[MethodImpl(MethodImplOptions.InternalCall)]

			[ClrSymcall(Symbol = "Object::GetSize", FullyQualified = true)]
			public extern int Run();
		}

		class MyStruct2
		{
			public void Run()
			{
				Console.WriteLine("giblet");
			}
		}

		static void nil()
		{
			const int OPCODES_OFS = 0xAB78E;

			var txtSeg = Segments.GetSegment(".text", "clr.dll");
			var ptr    = txtSeg.SectionAddress + OPCODES_OFS;

			Console.WriteLine(ptr.CopyOutBytes(5).AutoJoin());

			ptr.SafeWrite(new byte[5]);

			Console.WriteLine(ptr.CopyOutBytes(5).AutoJoin());
		}

		static void alt()
		{
			Console.WriteLine(new MyStruct().Run());
		}

		static void bind()
		{
			var c  = typeof(MyStruct);
			var m  = c.GetAnyMethod("Run");
			var md = m.GetMethodDesc();

			Symcall.BindQuick(c, "Run");
		}

		[HandleProcessCorruptedStateExceptions]
		public static void Main(string[] args)
		{
			Core.Setup();

			// .text:00000001800AC78E E8 D5 09 4B 00 call    ?BuildMethodTableThrowException@MethodTableBuilder@@AEAAXJII@Z
			// .text 0000000180001000	000000018070E000


			var myStruct2 = new MyStruct2();
			var fn        = typeof(MyStruct2).GetAnyMethod("Run");
			Console.WriteLine(fn.GetMethodDesc().Reference.RVA);

			var fn2 = typeof(Program).GetAnyMethod("GetCurrentProcess");
			Console.WriteLine(fn2.GetMethodDesc().Reference.RVA);

			nil();

			bind();

			alt();


			Core.Close();
		}
	}
}