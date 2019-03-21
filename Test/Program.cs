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


		static void freecpy<T>(Pointer<T> value) where T : class
		{
			var size = Unsafe.HeapSize(value.Reference) + IntPtr.Size;
			value.ZeroBytes(size);
			//value.Subtract(IntPtr.Size);
			Mem.Free(value);
		}

		static Pointer<T> memcpy<T>(T value) where T : class
		{
			var memory   = Unsafe.MemoryOf(value);
			int fullSize = memory.Length + IntPtr.Size;
			var ptr      = Mem.AllocUnmanaged<byte>(fullSize);

			// Write address of actual memory
			ptr.WriteAny(ptr.Address + (IntPtr.Size * 2));

			// Move forward
			ptr.Add(IntPtr.Size);

			// Write copied memory
			ptr.WriteAll(memory);

			// Move back
			ptr.Subtract(IntPtr.Size);

			return ptr.Reinterpret<T>();
		}


		// https://github.com/dotnet/coreclr/blob/1f3f474a13bdde1c5fecdf8cd9ce525dbe5df000/src/vm/reflectioninvocation.cpp#L2970
		static bool HasFlagFast<TEnum>(this TEnum value, TEnum flag)
		{
			var pThis  = Unsafe.AddressOf(ref value);
			var pFlags = Unsafe.AddressOf(ref flag);
			var size   = Unsafe.SizeOf<TEnum>();

			// var underlying = typeof(TEnum).GetEnumUnderlyingType();


			switch (size) {
				case sizeof(byte):
					return ((*(byte*) pThis & *(byte*) pFlags) == *(byte*) pFlags);
				case sizeof(ushort):
					return ((*(ushort*) pThis & *(ushort*) pFlags) == *(ushort*) pFlags);
				case sizeof(uint):
					return ((*(uint*) pThis & *(uint*) pFlags) == *(uint*) pFlags);
				case sizeof(ulong):
					return ((*(ulong*) pThis & *(ulong*) pFlags) == *(ulong*) pFlags);
				default:
					return false;
			}
		}

		[HandleProcessCorruptedStateExceptions]
		public static void Main(string[] args)
		{
			Global.Setup();
			Clr.ClrPdb = new FileInfo(@"C:\Symbols\clr.pdb");
			Clr.Setup();


			var list = new List<int>
			{
				1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16
			};

			var ptr = memcpy(list);
			Console.WriteLine(ptr);
			freecpy(ptr);
			var cpy = ptr.Value;

			Pointer<byte> str = Marshal.StringToHGlobalAnsi("foo");
			Console.WriteLine(str.ReadString(StringTypes.AnsiStr));

			string s      = "foo";
			var    module = Modules.FromAddress(typeof(string).GetMethodTable().Address);
			Console.WriteLine(module);

			var fn = ClrFunctions.GetClrFunctionAddress("JIT_GetRuntimeType");
			Console.WriteLine(Modules.FromAddress(fn));

			BindingFlags value = BindingFlags.Public | BindingFlags.Static;
			Console.WriteLine(value.HasFlagFast(BindingFlags.Public));


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