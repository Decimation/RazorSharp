#region

using System;
using System.Diagnostics;
using System.Linq;
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
using RazorSharp.Reflection;
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

		[HandleProcessCorruptedStateExceptions]
		public static void Main(string[] args)
		{
			Global.Value.WriteDebug("g", "hi!");

			var rField = typeof(string).GetAnyField("m_firstChar");
			var mField = (MetaField) rField;

			Console.WriteLine(mField);
		}
	}
}