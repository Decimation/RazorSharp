//using Microsoft.Diagnostics.Runtime;
// ReSharper disable InconsistentNaming

#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using RazorSharp;
using RazorSharp.Analysis;
using RazorSharp.Clr;
using RazorSharp.Clr.Meta;
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


	public static unsafe class Program
	{
#if DEBUG
		static Program()
		{
			Conditions.CheckCompatibility();
			ClrFunctions.Init();
		}
#endif

		// todo: protect address-sensitive functions
		// todo: replace native pointers* with Pointer<T> for consistency
		// todo: RazorSharp, ClrMD, Reflection comparison

		private static object _static;
		
		[HandleProcessCorruptedStateExceptions]
		public static void Main(string[] args)
		{
			Console.WriteLine(Constants.IS_64_BIT);
			Console.WriteLine(IntPtr.Size);
			Console.WriteLine(Offsets.PTR_SIZE);
			Console.WriteLine(Environment.Is64BitProcess);
			Console.WriteLine(Unsafe.AddressOf(ref _static));
			Console.WriteLine("\n-\n");

			var fd=Meta.GetType(typeof(Program)).Fields["_static"];
			Console.WriteLine(fd.GetStaticAddr());
			
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
			

			Pointer<int> ptr = Marshal.UnsafeAddrOfPinnedArrayElement(irg, 1);
			Console.WriteLine(ptr);

			var m = Meta.GetType<int>();
			Console.WriteLine(m);
			Debug.Assert(Compare<int>());
		}

		[Flags]
		private enum Flags
		{
			One = 1,
			Two = 2,
			Three = 4
		}

		private static bool HasFlagFast(this Flags v, Flags f) { return (v & f) == f; }

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