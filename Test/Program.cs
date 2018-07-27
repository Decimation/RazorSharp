using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using BenchmarkDotNet.Running;
using NUnit.Framework;
using RazorCommon;
using RazorCommon.Strings;
using RazorSharp;
using RazorSharp.Analysis;
using RazorSharp.Experimental;
using RazorSharp.Memory;
using RazorSharp.Pointers;
using RazorSharp.Runtime;
using RazorSharp.Runtime.CLRTypes;
using RazorSharp.Runtime.CLRTypes.HeapObjects;
using Test.Testing;
using Test.Testing.Benchmarking;
using Unsafe = RazorSharp.Unsafe;
using static RazorSharp.Utilities.Assertion;
using Assertion = RazorSharp.Utilities.Assertion;

namespace Test
{

	using CSUnsafe = System.Runtime.CompilerServices.Unsafe;

	internal static unsafe class Program
	{

#if DEBUG
		static Program()
		{
			StandardOut.ModConsole();
		}
#endif


		/**
		 * RazorSharp
		 *
		 * History:
		 * 	- RazorSharp (deci-common-c)
		 * 	- RazorSharpNeue
		 * 	- RazorCLR
		 * 	- RazorSharp
		 *
		 * RazorSharp:
		 *  - RazorCommon
		 * 	- CompilerServices.Unsafe
		 *  - RazorInvoke
		 *  - Fody
		 *  - MethodTimer Fody
		 *
		 * Test:
		 *  - RazorCommon
		 *  - CompilerServices.Unsafe
		 * 	- NUnit
		 *  - BenchmarkDotNet
		 *  - Fody
		 *  - MethodTimer Fody
		 *
		 * Notes:
		 *  - 32-bit is not fully supported
		 *  - Most types are not thread-safe
		 *
		 * Goals:
		 *  - Provide identical functionality of ClrMD, SOS, and Reflection
		 * 	  but in a faster and more efficient way
		 */
		public static void Main(string[] args)
		{




			// MethodDesc Table
			// Entry 			MethodDesc    	 JIT 	Name
			// 00007fff1bf70660 00007fff1bb6cf10 PreJIT System.Collections.Generic.List`1[[System.Int32, mscorlib]]..cctor()
			// 00007fff1c900a40 00007fff1bb6ccb8 PreJIT System.Collections.Generic.List`1[[System.Int32, mscorlib]]..ctor()
			// 00007fff1bf70600 00007fff1bb6ccc0 PreJIT System.Collections.Generic.List`1[[System.Int32, mscorlib]]..ctor(Int32)
			// 00007fff1bf497d0 00007fff1bb6ccc8 PreJIT System.Collections.Generic.List`1[[System.Int32, mscorlib]]..ctor(System.Collections.Generic.IEnumerable`1)

		}



	}

}