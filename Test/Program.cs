#region

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Reflection.Emit;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using RazorCommon;
using RazorCommon.Diagnostics;
using RazorCommon.Extensions;
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
using Test.PEFile;
using CSUnsafe = System.Runtime.CompilerServices.Unsafe;
using Unsafe = RazorSharp.Memory.Unsafe;

#endregion


namespace Test
{
	#region

	using DWORD = UInt32;

	#endregion

	using Ptr = Pointer<byte>;

	public static unsafe partial class Program
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

		private interface IIndexable<T>
		{
			T this[int i] { get; set; }
		}

		private class MyStruct
		{
			// #define IDS_CLASSLOAD_MISSINGMETHODRVA          0x1797

			// MethodTableBuilder::ValidateMethods()
			// https://github.com/dotnet/coreclr/blob/master/src/vm/methodtablebuilder.cpp
			// 4880: BuildMethodTableThrowException(IDS_CLASSLOAD_MISSINGMETHODRVA, it.Token());
//			[MethodImpl(MethodImplOptions.InternalCall)]


			[ClrSymcall(Symbol = "Object::GetSize", FullyQualified = true)]
			public int Run()
			{
				throw null;
			}
		}


		private enum MyEnum : ulong
		{
			VAL
		}

		private static ulong Ul<T>(T v) where T : struct
		{
			return default;
		}
		
		[HandleProcessCorruptedStateExceptions]
		public static void Main(string[] args)
		{
			Core.Setup();

			var mc = new MyClass {s = "foo"};

			Console.WriteLine(mc);

			var n = Unsafe.DeepCopy(mc);
			Console.WriteLine(n);

			CString* cs;
			Ul(MyEnum.VAL);

			Conditions.Assert(Unsafe.Unbox<int>(1) == 1);

			Core.Close();
		}

		private struct CString { }


		private static void Arglist(__arglist)
		{
			var rg = new ArgIterator(__arglist);

			while (rg.GetRemainingCount() > 0) {
				var v = rg.GetNextArg();
				Console.WriteLine(TypedReference.ToObject(v));
			}
		}
	}
}