#region

using System;
using System.Buffers;
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

		class MyClass2
		{
			public readonly string Sz = "foo";

			public void change()
			{
				Unsafe.Set(this, "Sz", "bar");
			}
		}

		struct MyStruct2
		{
			public string Sz;


			public void change()
			{
				Unsafe.Set(ref this, "Sz", "bar");
			}
		}

		

		[HandleProcessCorruptedStateExceptions]
		public static void Main(string[] args)
		{
			Core.Setup();

			var ms = typeof(string).GetMetaType();
			Console.WriteLine(ms);
			Console.WriteLine(ms["m_firstChar"]);


			Console.WriteLine("done");
			var m = new MyClass2();
			Console.WriteLine(m.Sz);
			Console.WriteLine();

			m.change();

			Console.WriteLine(m.Sz);
			
			

			var ms2 = new MyStruct2 {Sz = "fooby"};
			Console.WriteLine(ms2.Sz);
			ms2.change();
			Console.WriteLine(ms2.Sz);
			Console.WriteLine(ms2.Sz = "barlet");
			


			Core.Close();
		}

		private struct CString { }

		private static void ReadToken(uint u) { }

		private static string ReadCString(byte[] buf)
		{
			return Encoding.ASCII.GetString(buf);
		}


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