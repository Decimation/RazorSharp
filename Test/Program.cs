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
using RazorSharp.Native;
using RazorSharp.Native.Symbols;
using RazorSharp.Pointers;
using RazorSharp.Utilities;
using CSUnsafe = System.Runtime.CompilerServices.Unsafe;
using Unsafe = RazorSharp.Unsafe;

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


		// todo: reorganize namespaces and fix access levels

		[HandleProcessCorruptedStateExceptions]
		public static void Main(string[] args)
		{
			Core.Setup();


			string img = Clr.ClrPdb.FullName;
			Console.WriteLine(img);
			var e = new EnumSymbols();
			var sw = Stopwatch.StartNew();
			e.LoadAll(img);
			sw.Stop();
			Console.WriteLine(sw.Elapsed);
			var syms = e.Symbols;
			var x    = syms.First(s => s.Name.Contains("g_lowest_address"));
			Console.WriteLine(x);
			Console.WriteLine("base {0:X}", x.ModBase);
			Console.WriteLine("diff {0:X}", x.Address - x.ModBase);

			using (var sym = new Symbols(img)) {
				var s = sym.GetSymOffset("g_lowest_address");
				Console.WriteLine("{0:X}", s);
			}

			long o = e.Symbols.First(y => y.Name.Contains("g_lowest_address")).Offset;
			Console.WriteLine(o);
			Console.WriteLine(e.Symbols.First(n => n.Name.Contains("g_pStringClass")));

			var sm = new Symbols(img);
			
			var sw2 = Stopwatch.StartNew();
			long i = sm.GetSymOffset("?g_pStringClass@@3PEAVMethodTable@@EA");
			sw2.Stop();
			Console.WriteLine(sw2.Elapsed);
			sm.Dispose();

			Core.Close();
		}
	}
}