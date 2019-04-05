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
using RazorSharp.Native.Structures.Symbols;
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

		[DllImport("kernel32")]
		private static extern IntPtr GetProcessHeap();

		[DllImport("kernel32")]
		private static extern uint GetProcessHeaps(uint nHeaps, IntPtr[] handles);

		private static IntPtr[] GetProcessHeaps()
		{
			var  p = new IntPtr[256];
			uint n = GetProcessHeaps((uint) p.Length, p);
			Array.Resize(ref p, (int) n);
			return p;
		}

		
		
	
		// todo: reorganize namespaces and fix access levels
		
		[HandleProcessCorruptedStateExceptions]
		public static void Main(string[] args)
		{
			Core.Setup();

			var sym = new SymCollection(Clr.ClrPdb.FullName,"*");
			sym.LoadAll();

			foreach (var s in sym.Search("MethodTable")) {
				Console.WriteLine("{0}: {1} {2}",s.Name,s.TypeIndex,s.SizeOfStruct);
			}

			var g = sym.Search("g_pCanonMethodTableClass").First();
			
			Console.WriteLine(g.TypeIndex);
			
			

			
			


			Core.Close();
		}
	}
}