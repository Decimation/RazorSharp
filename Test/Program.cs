﻿//using Microsoft.Diagnostics.Runtime;
// ReSharper disable InconsistentNaming

#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.IO;
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
using RazorSharp.Memory.Calling.Sym;
using RazorSharp.Memory.Calling.Sym.Attributes;
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

		[ClrSymcall(Symbol = "Object::GetSize", FullyQualified = true)]
		private static int Size(this object obj)
		{
			return Unsafe.INVALID_VALUE;
		}


		static Delegate get(string name)
		{
			using (var sym = new Symbolism(Symbolism.CLR_PDB)) {
				var addr = sym.GetSymAddress(name, "clr.dll");
				return Marshal.GetDelegateForFunctionPointer(addr.Address, typeof(Delegate));
			}
		}


		[HandleProcessCorruptedStateExceptions]
		public static void Main(string[] args)
		{
			Global.Setup();
//			Clr.Setup();
			Symcall.Setup();

			Symcall.BindQuick(typeof(Program));


//			var str = "foo";
//			Conditions.Assert(str.Size() == Unsafe.HeapSize(str));

			Symcall.BindQuick(typeof(GCHeap));

			int[]        rg  = {1, 2, 3};
			Pointer<int> ptr = Marshal.UnsafeAddrOfPinnedArrayElement(rg, 1);
			Console.WriteLine("> {0:P}", ptr);
			//Pointer<byte> containing = GCHeap.GlobalHeap.Reference.GetContainingObject(ptr.ToPointer(), 0);
			//Console.WriteLine("> {0:P}",containing);


			// Shit is kinda broken
			var clrTypes = new[] {typeof(MethodDesc), typeof(FieldDesc), typeof(ClrFunctions), typeof(GCHeap)};
			foreach (var type in clrTypes) {
//				Symcall.BindQuick(type);
			}


			// Cursed number. Do not use!!!!!!!!!!!
			const int emergence = 177013;


			// SHUT IT DOWN
			Global.Close();
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