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
using RazorSharp.Pointers;
using RazorSharp.Runtime;
using Test.Testing;
using Unsafe = RazorSharp.Unsafe;
using static RazorSharp.Utilities.Assertion;

namespace Test
{

	using CSUnsafe = System.Runtime.CompilerServices.Unsafe;

	internal static unsafe class Program
	{

		static Program()
		{
			//StandardOut.ModConsole();
		}

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
		 */
		public static void Main(string[] args)
		{
			var summary = BenchmarkRunner.Run<ArrayPointerBenchmarking>();
		}



		private static void PrintTable<T>(ArrayPointer<T> arr)
		{
			for (int i = 0; i < arr.Count; i++) {
				Console.Clear();
				Console.Write("{0:T}", arr);
				arr++;
				Thread.Sleep(1000);
			}

			Console.ReadKey();
			Console.Clear();
			Console.Write("{0:T}", arr);
			Console.WriteLine(Hex.ToHex(arr.Address));
		}





	}

}