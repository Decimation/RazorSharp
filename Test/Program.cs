//using Microsoft.Diagnostics.Runtime;
// ReSharper disable InconsistentNaming

#region

using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Dynamic;
using System.Globalization;
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
using RazorCommon.Extensions;
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
using RazorSharp.Native.Enums;
using RazorSharp.Native.Structures;
using RazorSharp.Pointers;
using RazorSharp.Utilities;
using Test.Samples;
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

			return ptr.Cast<T>();
		}


		// https://github.com/dotnet/coreclr/blob/1f3f474a13bdde1c5fecdf8cd9ce525dbe5df000/src/vm/reflectioninvocation.cpp#L2970
		static bool HasFlagFast<TEnum>(this TEnum value, TEnum flag) where TEnum : Enum
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
					throw new Exception();
			}
		}

		[Flags]
		public enum PhoneService
		{
			None     = 0,
			LandLine = 1,
			Cell     = 2,
			Fax      = 4,
			Internet = 8,
			Other    = 16
		}

		static bool IsPowerOf2(int x)
		{
			// return ((x) && (!(x & (x - 1))));

			// Allow 0
			if (x == 0) {
				return true;
			}

			return ((Convert.ToBoolean(x)) && (!Convert.ToBoolean((x & (x - 1)))));
		}


		class Class
		{
			public int doSomething2()
			{
				return 2;
			}

			public int doSomething()
			{
				return 1;
			}
		}

		delegate Pointer<int> broCop();

		struct MyStruct
		{
			public char m_firstChar;
		}

		[HandleProcessCorruptedStateExceptions]
		public static void Main(string[] args)
		{
			Global.Setup();
			Clr.ClrPdb = new FileInfo(@"C:\Symbols\clr.pdb");
			Clr.Setup();

			var a = typeof(Class).GetAnyMethod("doSomething");
			var b = typeof(Class).GetAnyMethod("doSomething2");

			Functions.Swap(a, b);

			Debug.Assert(new Class().doSomething() == 2);

			Console.WriteLine(ClrFunctions.FindField(typeof(string), "m_firstChar"));
			Console.WriteLine(typeof(int[]).GetMetaType());


			var xy = new global::System.Single();


			var o = new {str = "foo", i = 1};


			var   m = typeof(float).GetMetaType();
			float f = 3.14f;
			Debug.Assert(m.Fields["m_value"].GetAddress(ref f) == &f);

			const string foo = nameof(foo);
			const string bar = nameof(bar);

			string bar2 = "bar";


			const string asmStr = "RazorSharp";
			var          asm    = Assembly.Load(asmStr);


			var sys     = Type.GetType("System.SZArrayHelper");
			var methods = sys.GetMetaType().Methods;
			foreach (var method in methods) {
				Console.WriteLine(method.MethodInfo);
			}

			var jit  = Type.GetType("System.Runtime.CompilerServices.JitHelpers");
			var cast = jit.GetAnyMethod("UnsafeCast");
			object obj = new[] {1, 2, 3};
			cast = cast.MakeGenericMethod(typeof(object));
			var output = cast.Invoke(null, new object[] {obj});
			Console.WriteLine(">> {0}",output);


			Debug.Assert(bar2.IsInterned());
			Debug.Assert(bar.IsInterned());

			Inspect.Heap(bar);
			Inspect.Heap(bar2);


			// SHUT IT DOWN
			Clr.Close();
			Global.Close();
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