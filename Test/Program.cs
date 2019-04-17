#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using InlineIL;
using RazorCommon;
using RazorCommon.Extensions;
using RazorCommon.Strings;
using RazorSharp;
using RazorSharp.CoreClr;
using RazorSharp.CoreClr.Structures;
using RazorSharp.CoreClr.Structures.ILMethods;
using RazorSharp.CoreJit;
using RazorSharp.Memory;
using RazorSharp.Memory.Calling.Symbols;
using RazorSharp.Memory.Calling.Symbols.Attributes;
using RazorSharp.Memory.Pointers;
using RazorSharp.Native;
using RazorSharp.Utilities;
using CSUnsafe = System.Runtime.CompilerServices.Unsafe;
using Unsafe = RazorSharp.Memory.Unsafe;

#endregion


namespace Test
{
	#region

	using DWORD = UInt32;
	using Ptr = Pointer<byte>;

	#endregion


	public static unsafe class Program
	{
		// Common library: RazorCommon
		// Testing library: Sandbox


		private static void __Compile(Type t, string n)
		{
			RuntimeHelpers.PrepareMethod(t.GetAnyMethod(n).MethodHandle);
		}

		

		public static void Main(string[] args)
		{
			// ICorJitCompiler
			var pJit = CorJitCompiler.GetJit();

			ModuleInitializer.GlobalSetup();

			
			


			ModuleInitializer.GlobalClose();

			// hwnd 000B0B74
			// SunAwtFrame
		}
	}
}