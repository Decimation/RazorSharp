#region

using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using RazorSharp.CoreClr;
using RazorSharp.CoreClr.Meta;
using RazorSharp.CoreClr.Structures;
using RazorSharp.Interop;
using RazorSharp.Memory;
using RazorSharp.Memory.Pointers;
using RazorSharp.Utilities;

#endregion


namespace Test
{
	#region

	using DWORD = UInt32;

	#endregion


	public static unsafe class Program
	{
		// Common library: SimpleSharp
		// Testing library: Sandbox

		public static void Run<T>()
		{
			Console.WriteLine(">> {0}",typeof(T));
		}
		
		[HandleProcessCorruptedStateExceptions]
		public static void Main(string[] args)
		{
			
			Run<int>();
			Functions.CallGenericMethod(typeof(Program).GetMethod(nameof(Run)), typeof(int), null);
		}
	}
}