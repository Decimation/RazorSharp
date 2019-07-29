#region

using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using RazorSharp.CoreClr;
using RazorSharp.CoreClr.Meta;
using RazorSharp.CoreClr.Structures;
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


		struct MyStruct
		{
			private int   a;
			private short b;
		}

		class MyClass
		{
			public static string v = "foo";
		}

		[HandleProcessCorruptedStateExceptions]
		public static void Main(string[] args)
		{
			
			Process p;
		}
	}
}