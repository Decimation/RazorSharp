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
using RazorSharp.Interop;
using RazorSharp.Memory;
using RazorSharp.Memory.Pointers;
using RazorSharp.Utilities;
using System.Linq;
using RazorSharp.Core;
using RazorSharp.Interop.Utilities;

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
			var f = (MetaField) typeof(string).GetAnyField("m_firstChar");
			Console.WriteLine("{0:A}",f);

			var l = Runtime.AllocObject<List<int>>();
			l.Add(1);
			Console.WriteLine(l.Count);
			Console.WriteLine(GCHeap.GCCount);
			GC.Collect();
			Console.WriteLine(GCHeap.GCCount);
		}
	}
}