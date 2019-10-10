#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using RazorSharp;
using RazorSharp.Analysis;
using RazorSharp.CoreClr;
using RazorSharp.CoreClr.Meta;
using RazorSharp.CoreClr.Structures;
using RazorSharp.Interop;
using RazorSharp.Memory;
using RazorSharp.Memory.Pointers;
using RazorSharp.Utilities;
using System.Linq;

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

		static void Hello() { }

		[HandleProcessCorruptedStateExceptions]
		public static void Main(string[] args)
		{
			MetaMethod t = typeof(Program).GetAnyMethod(nameof(Hello));
			Console.WriteLine("{0:M}", t);

			MetaField f = typeof(string).GetAnyField("m_firstChar");
			Console.WriteLine("{0:M}", f);

			MetaType s = typeof(string);
			Console.WriteLine("{0:M}", s);
		}
	}
}