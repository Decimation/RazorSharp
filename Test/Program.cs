//using Microsoft.Diagnostics.Runtime;
// ReSharper disable InconsistentNaming

#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using NUnit.Framework;
using RazorCommon;
using RazorCommon.Strings;
using RazorCommon.Utilities;
using RazorSharp;
using RazorSharp.Analysis;
using RazorSharp.Clr;
using RazorSharp.Clr.Meta;
using RazorSharp.Clr.Structures;
using RazorSharp.Clr.Structures.HeapObjects;
using RazorSharp.Experimental;
using RazorSharp.Memory;
using RazorSharp.Memory.Attributes;
using RazorSharp.Native;
using RazorSharp.Pointers;
using RazorSharp.Utilities;
using Test.Testing;
using Constants = RazorSharp.Clr.Constants;
using Unsafe = RazorSharp.Unsafe;

#endregion


namespace Test
{
	#region

	using DWORD = UInt32;
	using CSUnsafe = System.Runtime.CompilerServices.Unsafe;

	#endregion


	[StructLayout(LayoutKind.Sequential)]
	public static unsafe class Program
	{
#if DEBUG
		static Program() { }
#endif

		// todo: protect address-sensitive functions
		// todo: replace native pointers* with Pointer<T> for consistency
		// todo: RazorSharp, ClrMD, Reflection comparison

		private static object _static;
		private const  string CONST_STR = "foo";

		private static void init()
		{
			// EED2
			//71470000

			// 8B D1 F6 C2 2 F 85 3A 2C 0 0 8B 42 18 8B 48 4 F6 C1 1 F 84 C0 15 9 0 8B 41 FF C3 90 90 51 3B CA F 84 CA 16 18 0 85 C9 74 8 85

			//Console.WriteLine(Meta.GetType<Struct>().Fields[0].Size);


			//Conditions.AssertAllEqualQ(Offsets.PTR_SIZE, IntPtr.Size, sizeof(void*), 8);
			//Conditions.Assert(Environment.Is64BitProcess);
			Conditions.CheckCompatibility();

			//ClrFunctions.Init();
			Console.OutputEncoding = Encoding.Unicode;
		}

		class Anime
		{
			private const int CONST = 1;

			public string foo()
			{
				return "foo";
			}

			public string bar()
			{
				return "bar";
			}
		}

		


		[HandleProcessCorruptedStateExceptions]
		public static void Main(string[] args)
		{
			// STAAARTTTT IIITTT UUUPPPP
			init();
//			Clr.Setup();

			Symcall.Setup();

			var fn = Symcall.GetClrFunctionAddress("MethodDesc::SetStableEntryPointInterlocked");
			
			string opCodes = "55 8B EC 53 56 57 8B D9 E8 11 68 F8 FF";
			var    ss      = new SigScanner();
			ss.SelectModule("clr.dll");
			Pointer<byte> ptr = ss.FindPattern(opCodes);
			Console.WriteLine(ptr);
			var ofs = ptr.Address.ToInt64() - Modules.GetModule("clr.dll").BaseAddress.ToInt64();
			Console.WriteLine("offset {0:X}",ofs);
			Console.WriteLine("ptr {0:P} {1:X}",fn, fn[0]);
			

			
			
			var fnx = Marshal.GetDelegateForFunctionPointer<Symcall.SetStableEntryPointInterlockedDelegate>(ptr.Address);

			var foo = typeof(Anime).GetMethod("foo");
			


			
			
			fnx(foo.MethodHandle.Value.ToPointer(), IntPtr.Zero);
			Console.ReadLine();
			

			


			// Shit is kinda broken
			try {
				Symcall.BindQuick(typeof(MethodDesc));
				Console.WriteLine("DONE!!!!");
			}
			catch (SEHException e) {
				Console.WriteLine(e);
				Console.WriteLine("{0:X}", e.HResult);
				Console.WriteLine(e.Message);
				
			}


			//var types = new[] {typeof(FieldDesc), typeof(MethodDesc), typeof(ClrFunctions), typeof(GCHeap)};
			//foreach (var t in types) {
			//	Symcall.BindQuick(t);
			//}


			// Cursed number. Do not use!!!!!!!!!!!
			const int emergence = 177013;

			// SHUT IT DOWN
			Global.Close();

			/*
			int[] rg = {1, 2, 3};
			Inspect.Heap<int[], int>(rg);


			string s = "foo";
			Inspect.Heap<string, byte>(s);

			string[] str = {"foo", "bar"};
			Inspect.Heap<string[], string>(str);

			int i = 0;
			Inspect.Stack(ref i);

			Inspect.Stack(ref s);

			Console.WriteLine(PrettyPrint.GenericName(typeof(KeyValuePair<int,string>)));

			int[] irg = {1, 2, 3};
			var obj = Runtime.GetArrayObject(ref irg);
			

			var m = Meta.GetType<int>();
			Console.WriteLine(m);
			Debug.Assert(Compare<int>());*/
		}


		[StructLayout(LayoutKind.Explicit)]
		struct Struct
		{
			[FieldOffset(0)]
			public void* m_int;

			[FieldOffset(12)]
			public int m_int2;

			[FieldOffset(12)]
			public int m_int3;
		}

		private delegate Type JIT_GetRuntimeType_MaybeNull(long mt);

		static string LayoutString<T>()
		{
			var type   = Meta.GetType<T>();
			var table  = new ConsoleTable("Name", "Type", "Offset", "Size");
			var fields = type.Fields.OrderBy(x => x.Offset).ToList();

			foreach (var field in fields) {
				table.AddRow(field.Name, field.FieldType.Name, field.Offset, field.Size);
			}

			return table.ToMarkDownString();
		}


		[Flags]
		private enum Flags
		{
			One   = 1,
			Two   = 2,
			Three = 4
		}

		private static bool HasFlagFast(this Flags v, Flags f)
		{
			return (v & f) == f;
		}

		private static bool Compare<T>()
		{
			return Compare(typeof(T), Meta.GetType<T>());
		}

		private static bool Compare(Type t, MetaType m)
		{
			bool[] rg =
			{
				t.Name == m.Name,
				t.IsArray == m.IsArray,
				t == m.RuntimeType
			};
			return rg.All(b => b);
		}


		private static void Dump<T>(T t, int recursivePasses = 0)
		{
			FieldInfo[] fields = Runtime.GetFields(t.GetType());

			var ct = new ConsoleTable("Field", "Type", "Value");
			foreach (var f in fields) {
				var    val = f.GetValue(t);
				string valStr;
				if (f.FieldType == typeof(IntPtr)) {
					valStr = Hex.TryCreateHex(val);
				}
				else if (val != null) {
					if (val.GetType().IsArray)
						valStr  = Collections.CreateString((Array) val, ToStringOptions.Hex);
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