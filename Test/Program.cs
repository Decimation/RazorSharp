using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
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
		 *
		 * Test:
		 *  - RazorCommon
		 *  - CompilerServices.Unsafe
		 * 	- NUnit
		 */
		public static void Main(string[] args)
		{
			/*string        a   = "anime";
			int           x   = 0xFF;
			Pointer<char> ptr = a;

			MemoryLayout.Step<char>(ptr.Address, a.Length);
			string[] arr = {"foo", StringUtils.Random(10), StringUtils.Random(15)};
			Pointer<string> arrPtr = arr;
			MemoryLayout.Step<byte>(arrPtr.Address, arr.Length * 8);*/

			string s = "foo";


			// @start 	0x1EAA6BC5AA4
			// @end		0x1EAA6BC5AA8

			//227633200 32767 3 7274598 111 0 0 -2147483648 227633200 32767

			int[] intArr = {1, 2, 3, 4, 5};

			/*
			ArrayPointer<char> arr = new ArrayPointer<char>(ref s);
			Debug.Assert(arr.Count == s.Length);
			Inspector<string> inspector = new ReferenceInspector<string>(ref s, InspectorMode.Address);
			Console.WriteLine(inspector);

			Console.WriteLine("{0:T}",arr);
			Console.WriteLine();

			ArrayPointer<int> intArrptr = new ArrayPointer<int>(ref intArr);
			Debug.Assert(intArrptr.Count == intArr.Length);
			Inspector<int[]> intInspector = new ReferenceInspector<int[]>(ref intArr, InspectorMode.Address);
			Console.WriteLine(intInspector);
			Console.WriteLine("{0:T}",intArrptr);*/

			ArrayPointer<int> decayInt = intArr;
			Debug.Assert(decayInt.Count == intArr.Length);
			Assertion.AssertElements(decayInt, intArr);


			AssertThrows<Exception>(delegate
			{
				var x = decayInt[-1];
			});



			AssertThrows<Exception>(delegate { decayInt += decayInt.Count + 1; });

			AssertThrows<Exception>(delegate { decayInt -= 2; });



			AssertThrows<Exception>(delegate
			{
				var x = decayInt[5];
			});


			ArrayPointer<char> decayStr = s;
			Debug.Assert(decayStr.Count == s.Length);
		}

		private static void PrintTable<T>(ArrayPointer<T> arr)
		{
			for (int i = 0; i < arr.Count; i++) {
				Console.Clear();
				Console.Write("{0:E}", arr);
				arr++;
				Thread.Sleep(500);
			}

			arr.MoveToStart();
			Console.Clear();
		}


		static Program()
		{
			StandardOut.ModConsole();
		}


	}

}