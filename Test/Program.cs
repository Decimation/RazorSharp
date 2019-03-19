//using Microsoft.Diagnostics.Runtime;
// ReSharper disable InconsistentNaming

#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Dynamic;
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
using RazorSharp.CoreClr;
using RazorSharp.CoreClr.Meta;
using RazorSharp.CoreClr.Structures;
using RazorSharp.Memory;
using RazorSharp.Memory.Calling.Symbols;
using RazorSharp.Memory.Calling.Symbols.Attributes;
using RazorSharp.Native;
using RazorSharp.Pointers;
using RazorSharp.Utilities;
using Test.Testing;
using Constants = RazorSharp.CoreClr.Constants;
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
		static Program() { }
#endif

		// todo: protect address-sensitive functions
		// todo: replace native pointers* with Pointer<T> for consistency
		// todo: RazorSharp, ClrMD, Reflection comparison


		[ClrSymcall(Symbol = "Object::GetSize", FullyQualified = true)]
		private static int Size(this object obj)
		{
			return Unsafe.INVALID_VALUE;
		}

		interface IClass
		{
			[ClrSymcall(Symbol = "Object::GetSize", FullyQualified = true)]
			int Size();
		}

		[StructLayout(LayoutKind.Explicit)]
		struct Struct
		{
			[FieldOffset(0)]
			public IntPtr _object;

			[FieldOffset(0)]
			public IntPtr _string;
		}

		public class Class
		{
			public char Char;
		}


		[HandleProcessCorruptedStateExceptions]
		public static void Main(string[] args)
		{
			Global.Setup();
			Clr.ClrPdb = new FileInfo(@"C:\Symbols\clr.pdb");
			Clr.Setup();

			var    s   = new Struct();
			string str = "foo";
			s._string = CSUnsafe.As<string, IntPtr>(ref str);
			var value = CSUnsafe.As<IntPtr, object>(ref s._object);

			Class c = (object) str as Class;
			Console.WriteLine(c);

			Pointer<string> rgString = AllocHelper.Alloc<string>(10);
			Console.WriteLine(AllocHelper.GetSize(rgString));
			Console.WriteLine(AllocHelper.GetLength(rgString));

			AllocHelper.Free(rgString);

			Pointer<int> bptr   = Mem.AllocUnmanaged<byte>(sizeof(int));
			var          valuei = bptr.Read();
			Console.WriteLine(bptr.Reinterpret<byte>().ToTable(4));

			AllocPointer<int> p = AllocHelper.Alloc<int>(5);
			Console.WriteLine(p.Offset);
			p.Pointer.WriteAll(1, 2, 3, 4, 5);
			for (int i = 0; i < p.Length; i++) {
				AllocHelper.Info(p.Pointer);
				p++;
			}
			p.Clear();

			

			foreach (int x in p) {
				Console.WriteLine(x);
			}


//			MemoryMarshal
//			Marshal
//			BitConverter
//			Convert
//			Span
//			Memory
//			Buffer
//			etc

			// SHUT IT DOWN
			Clr.Close();
			Global.Close();
		}


		static string LayoutString<T>()
		{
			var type   = typeof(T).GetMetaType();
			var table  = new ConsoleTable("Name", "Type", "Offset", "Size");
			var fields = type.Fields.OrderBy(x => x.Offset).ToList();


			foreach (var field in fields) {
				table.AddRow(field.Name, field.FieldType.Name, field.Offset, field.Size);
			}

			return table.ToMarkDownString();
		}


		private static bool Compare<T>()
		{
			return Compare(typeof(T), typeof(T).GetMetaType());
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