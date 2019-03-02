#region

//using Microsoft.Diagnostics.Runtime;

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
using Pastel;
using RazorCommon;
using RazorSharp;
using RazorSharp.Analysis;
using RazorSharp.CLR;
using RazorSharp.CLR.Fixed;
using RazorSharp.CLR.Meta;
using RazorSharp.CLR.Structures;
using RazorSharp.CLR.Structures.EE;
using RazorSharp.Experimental;
using RazorSharp.Memory;
using RazorSharp.Native;
using RazorSharp.Pointers;
using RazorSharp.Utilities;
using Test.Testing;
using static RazorSharp.Unsafe;
using Constants = RazorCommon.Constants;
using Unsafe = System.Runtime.CompilerServices.Unsafe;

#endregion

// ReSharper disable InconsistentNaming

#endregion

namespace Test
{
	#region

	using DWORD = UInt32;
	using CSUnsafe = Unsafe;

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

		[HandleProcessCorruptedStateExceptions]
		public static void Main(string[] args)
		{
			int[] rg = {1, 2, 3};
			Inspect.Heap<int[], int>(rg);


			string s = "foo";
			Inspect.Heap<string, byte>(s);

			string[] str = {"foo", "bar"};
			Inspect.Heap<string[], string>(str);

			int i = 0;
			Inspect.Stack(ref i);
			
			Inspect.Stack(ref s);
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


		private static void dump<T>(T t, int recursivePasses = 0)
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
					valStr = Constants.NULL_STR;
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