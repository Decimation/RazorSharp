#region

using System;
using System.Collections.Generic;
using System.Configuration;
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
using RazorSharp.Core;
using RazorSharp.Native;

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

		[HandleProcessCorruptedStateExceptions]
		public static void Main(string[] args)
		{
			PEHeaderReader r = new PEHeaderReader(Clr.Value.Module.FileName);

			foreach (var info in r.ImageSectionHeaders) {
				Console.WriteLine(info.Section);
			}
		}
	}
}