#region

using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using InlineIL;
using SimpleSharp;
using SimpleSharp.Diagnostics;
using SimpleSharp.Extensions;
using SimpleSharp.Strings;
using SimpleSharp.Utilities;
using RazorSharp;
using RazorSharp.CoreClr;
using RazorSharp.CoreClr.Structures;
using RazorSharp.CoreClr.Structures.ILMethods;
using RazorSharp.Memory;
using RazorSharp.Memory.Pointers;
using RazorSharp.Native;
using RazorSharp.Native.Symbols;
using RazorSharp.Native.Win32;
using RazorSharp.Utilities;
using CSUnsafe = System.Runtime.CompilerServices.Unsafe;
using Unsafe = RazorSharp.Memory.Unsafe;
using System.Net.Http;
using System.Runtime.ExceptionServices;
using JetBrains.Annotations;
using RazorSharp.Analysis;
using RazorSharp.CoreClr.Meta;
using RazorSharp.CoreClr.Structures.EE;
using RazorSharp.CoreClr.Structures.Enums;
using RazorSharp.Import;

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


		private static void Test<T>(T value)
		{
			var options = InspectOptions.Values | InspectOptions.FieldOffsets
			                                    | InspectOptions.Addresses
			                                    | InspectOptions.InternalStructures
			                                    | InspectOptions.MemoryOffsets
			                                    | InspectOptions.AuxiliaryInfo
			                                    | InspectOptions.ArrayOrString;

			var layout = Inspect.Layout<T>(InspectOptions.Types);
			layout.Options |= options;
			layout.Populate(ref value);
			Console.WriteLine(layout);
		}

		private static int Add(int a, int b)
		{
			Console.WriteLine("hello");
			return a + b;
		}
		
		private static int get()
		{
			return 0xFF;
		}
		
		

		[HandleProcessCorruptedStateExceptions]
		public static void Main(string[] args)
		{
			Global.SuppressLogger();

			var fn = typeof(Program).GetAnyMethod("Add");

			var control = new[]
			{
				/* IL_0 */ OpCodes.Nop,
				/* IL_1 */ OpCodes.Ldarg_0,
				/* IL_2 */ OpCodes.Ldarg_1,
				/* IL_3 */ OpCodes.Add,
				/* IL_4 */ OpCodes.Stloc_0,
				/* IL_5 */ OpCodes.Br_S, /* IL_7 */
				/* IL_7 */ OpCodes.Ldloc_0,
				/* IL_8 */ OpCodes.Ret
			};

			Console.WriteLine(InspectIL.ViewOpCode(InspectIL.AllOpCodes.First(f => f.Size == 2)));
			fixed (byte* p = new byte[]{0x0,0xFE})
				Console.WriteLine(InspectIL.GetOpCodes(p,2)[0]);
			
			Console.WriteLine(BitConverter.GetBytes(OpCodes.Ldarg_S.Value).AutoJoin());
			Console.WriteLine(InspectIL.ILString(fn.GetMethodBody().GetILAsByteArray()));

//			Console.WriteLine(InspectIL.ILString(typeof(Program).GetAnyMethod(nameof(get)).GetMethodBody().GetILAsByteArray()));


			var il = fn.GetMethodBody().GetILAsByteArray();

			fixed (byte* p = il) {
				var rg = InspectIL.GetOpCodes(p, il.Length);
				foreach (var c in rg) {
					Console.WriteLine(c.Name);
				}
			}

			foreach (var ins in InspectIL.GetInstructions(fn)) {
				Console.WriteLine(ins);
			}
		}
	}
}