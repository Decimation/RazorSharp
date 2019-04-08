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


		[HandleProcessCorruptedStateExceptions]
		public static void Main(string[] args)
		{
			Core.Setup();

			// .text:00000001800AC78E E8 D5 09 4B 00 call    ?BuildMethodTableThrowException@MethodTableBuilder@@AEAAXJII@Z
			// .text 0000000180001000	000000018070E000


			decimal d = Decimal.MaxValue;
			Inspect.Layout(ref d);

			decimal x = Decimal.MinValue;
			Inspect.Layout(ref x);

			string str = "foo";
			Inspect.Layout(ref str);

			

			Console.WriteLine(Hex.TryCreateHex(Math.PI));
			
			
			Arglist(__arglist(1,2,3,"fooblet"));

			Core.Close();
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