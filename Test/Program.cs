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

		[HandleProcessCorruptedStateExceptions]
		public static void Main(string[] args)
		{
			Core.Setup();

			var meta = typeof(Program).GetMetaType();

			var m = meta.Methods["GetCurrentProcess"];

			Console.WriteLine(m);
			Console.WriteLine(m.Flags);

			var m2 = meta.Methods["Size"];

			Console.WriteLine(m2);

			object str = "foo";
			var gc = GCHandle.Alloc(str, GCHandleType.Pinned);


			var s = Clr.ClrSymbols.Symbols.Where(sym => sym.Name.Contains("GetClassification")).ToList();
			Console.WriteLine(s.Count);
			
			gc.Free();
			
			

			Core.Close();
		}
	}
}