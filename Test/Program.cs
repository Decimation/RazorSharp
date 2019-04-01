﻿#region

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using RazorCommon;
using RazorCommon.Extensions;
using RazorCommon.Strings;
using RazorCommon.Utilities;
using RazorSharp;
using RazorSharp.Analysis;
using RazorSharp.CoreClr;
using RazorSharp.CoreClr.Enums;
using RazorSharp.CoreClr.Structures;
using RazorSharp.Memory;
using RazorSharp.Memory.Calling.Symbols.Attributes;
using RazorSharp.Native;
using RazorSharp.Native.Enums;
using RazorSharp.Native.Enums.ThreadContext;
using RazorSharp.Native.Structures;
using RazorSharp.Native.Structures.ThreadContext;
using RazorSharp.Pointers;
using RazorSharp.Utilities;
using CSUnsafe = System.Runtime.CompilerServices.Unsafe;
using ThreadState = System.Threading.ThreadState;
using Unsafe = RazorSharp.Unsafe;

#endregion


namespace Test
{
	#region

	using DWORD = UInt32;

	#endregion


	public static unsafe class Program
	{
		
		// todo: protect address-sensitive functions
		// todo: replace native pointers* with Pointer<T> for consistency
		// todo: RazorSharp, ClrMD, Reflection, Cecil comparison


		[ClrSymcall(Symbol = "Object::GetSize", FullyQualified = true)]
		private static int Size(this object obj)
		{
			return Constants.INVALID_VALUE;
		}

		class __Error
		{
			public __Error Air => this;
		}

		struct MString
		{
			public MString(string value) { }

			public static implicit operator MString(string value)
			{
				return new MString(value);
			}
		}

		struct QString
		{
			private Pointer<char> m_str;

			public QString(string value)
			{
				m_str = null;
			}

			public static implicit operator QString(string value)
			{
				return new QString(value);
			}
		}

		delegate void Run();

		

		[HandleProcessCorruptedStateExceptions]
		public static void Main(string[] args)
		{
			Global.Setup();
			Clr.ClrPdb = new FileInfo(@"C:\Symbols\clr.pdb");
			Clr.Setup();

//			const string asmStr = "RazorSharp";
//			var          asm    = Assembly.Load(asmStr);

			

			int* mem = stackalloc int[256];
			mem[0] = 1;
			mem[1] = 2;
			mem[2] = 3;
			mem[3] = 4;

			var memStream = new MemStream(mem);
			
			
			for (int i = 0; i < 3; i++) {
				
				int memValue    = memStream.Read<int>();
				int memValueDef = mem[i];
				
				Debug.Assert(memValue == memValueDef);
			}

			Console.WriteLine(memStream.Read<int>());


			string[][] matrix =
			{
				new[]
				{
					"goo", "sporg"
				},
				new[]
				{
					"sperg"
				}
			};

			var intMatrix = new[]
			{
				new byte[]
				{
					1, 2, 3
				},
				new byte[]
				{
					4, 5, 6
				},
				new byte[]
				{
					0xFF, 0xFF, 0xFF
				}
			};

			

			var ctx = Kernel32.GetContext(ContextFlags.CONTEXT_ALL);
			Console.WriteLine(ctx.Rax);

			
			var hThread = Kernel32.OpenThread(ThreadAccess.All, (int) Kernel32.GetCurrentThreadId());
			
			var t = new Thread(() =>
			{
				
				Kernel32.SuspendThread(hThread);
				
				
				
				
				ctx.Rax = 255;
				Kernel32.SetThreadContext(hThread, ref ctx);
				Console.WriteLine(">> Set thread ctx");
				
				Kernel32.ResumeThread(hThread);
				Console.WriteLine("done");
			});
			
			t.Start();

			
			
			Console.WriteLine("ok nigga");


			float pi = 3.14f;
			var val = MemConvert.UnionCast<double, float>(pi);
			Console.WriteLine(val);
			

			// SHUT IT DOWN
			Symbols.Close();
			Clr.Close();
			Global.Close();
		}


		class Class<K, V, E> { }

		static void loadClass()
		{
			try {
				var fn = typeof(Miscellaneous).GetAnyMethod("Run");
			}
			catch (TypeLoadException e) {
				Console.WriteLine("shitlet");
				Console.WriteLine(e);
				throw;
			}
		}


		static unsafe void wstrcpy(char* dmem, char* smem, int charCount)
		{
			Mem.Copy<byte>(dmem, smem, charCount * 2);
		}


		//This bypasses the restriction that you can't have a pointer to T,
		//letting you write very high-performance generic code.
		//It's dangerous if you don't know what you're doing, but very worth if you do.
		private static T Read<T>(IntPtr address)
		{
			var obj = default(T);
			var tr  = __makeref(obj);

			//This is equivalent to shooting yourself in the foot
			//but it's the only high-perf solution in some cases
			//it sets the first field of the TypedReference (which is a pointer)
			//to the address you give it, then it dereferences the value.
			//Better be 10000% sure that your type T is unmanaged/blittable...
			*(IntPtr*) (&tr) = address;

			return __refvalue(tr, T);
		}


		[DllImport(@"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\clr.dll")]
		private static extern void* GetCLRFunction(string str);


		private static void Dump<T>(T t, int recursivePasses = 0)
		{
			FieldInfo[] fields = t.GetType().GetMethodTableFields();

			var ct = new ConsoleTable("Field", "Type", "Value");
			foreach (var f in fields) {
				var    val = f.GetValue(t);
				string valStr;
				if (f.FieldType == typeof(IntPtr)) {
					valStr = Hex.TryCreateHex(val);
				}
				else if (val != null) {
					if (val.GetType().IsArray)
						valStr  = ((Array) val).AutoJoin(ToStringOptions.Hex);
					else valStr = val.ToString();
				}
				else {
					valStr = StringConstants.NULL_STR;
				}

				ct.AddRow(f.Name, f.FieldType.Name, valStr);
			}

			Console.WriteLine(ct.ToMarkDownString());
		}

		private static bool TryAlloc(object o, out GCHandle g)
		{
			try {
				g = GCHandle.Alloc(o, GCHandleType.Pinned);
				return true;
			}
			catch {
				g = default;
				return false;
			}
		}
	}
}