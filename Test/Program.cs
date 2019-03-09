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
using RazorCommon.Utilities;
using RazorSharp;
using RazorSharp.Analysis;
using RazorSharp.Clr;
using RazorSharp.Clr.Meta;
using RazorSharp.Clr.Structures;
using RazorSharp.Experimental;
using RazorSharp.Memory;
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
		static Program()
		{
			Clr.Reorganize();
			Conditions.CheckCompatibility();
			ClrFunctions.Init();
			Console.OutputEncoding = Encoding.Unicode;
		}
#endif

		// todo: protect address-sensitive functions
		// todo: replace native pointers* with Pointer<T> for consistency
		// todo: RazorSharp, ClrMD, Reflection comparison

		private static object _static;
		private const  string CONST_STR = "foo";

		[HandleProcessCorruptedStateExceptions]
		public static void Main(string[] args)
		{
			Conditions.AssertAllEqualQ(Offsets.PTR_SIZE, IntPtr.Size, sizeof(void*), 8);
			Conditions.Assert(Environment.Is64BitProcess);
			
			


			

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

		[StructLayout(LayoutKind.Explicit)]
		struct Struct
		{
			[FieldOffset(0)]
			public Pointer<int> m_int;

			[FieldOffset(8)]
			public int m_int2;

			public override string ToString()
			{
				return String.Format("m_int: {0:P} | m_int2: {1}", m_int, m_int2);
			}
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
					valStr = RazorCommon.Constants.NULL_STR;
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