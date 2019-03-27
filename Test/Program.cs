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
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Newtonsoft.Json.Converters;
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
using RazorSharp.CoreClr.Structures.HeapObjects;
using RazorSharp.Memory;
using RazorSharp.Memory.Calling.Symbols;
using RazorSharp.Memory.Calling.Symbols.Attributes;
using RazorSharp.Native;
using RazorSharp.Native.Enums;
using RazorSharp.Native.Structures;
using RazorSharp.Pointers;
using RazorSharp.Utilities;
using RazorSharp.Utilities.Exceptions;
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

		private static byte[] memoryOf(this object obj)
		{
			return Unsafe.MemoryOf(obj);
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
			public static int Static;

			static Class()
			{
				Static = Int32.MaxValue;
			}

			public int doSomething2()
			{
				return 2;
			}

			public int doSomething()
			{
				return 1;
			}
		}


		[StructLayout(LayoutKind.Sequential)]
		struct MyStruct
		{
			public Pointer<MethodTable> m_Pointer;
			public int                  m_len;
			public char                 m_firstChar;

			public override string ToString()
			{
				return m_firstChar.ToString();
			}
		}


		// https://referencesource.microsoft.com/#mscorlib/system/runtime/compilerservices/jithelpers.cs,42f2478cbfe5a17b

		private static void SetFieldOffset<T>(string name, int offset)
		{
			var field = typeof(T).GetMetaType()[name];
			field.Offset = offset;
		}

		[HandleProcessCorruptedStateExceptions]
		public static void Main(string[] args)
		{
			Global.Setup();
			Clr.ClrPdb = new FileInfo(@"C:\Symbols\clr.pdb");
			Clr.Setup();


			const string asmStr = "RazorSharp";
			var          asm    = Assembly.Load(asmStr);


			const string foo  = nameof(foo);
			var          cast = Unsafe.Cast<string, Pointer<StringObject>>(foo);


			cast.Add(8)
			    .Add(4)
			    .Cast<char>()
			    .WriteAll("g");

			Debug.Assert(foo.SequenceEqual("goo"));

			var csUnsafe = typeof(CSUnsafe);
			var il       = csUnsafe.GetMetaType().Methods["AsPointer"].GetILHeader();
			var ilCode   = csUnsafe.GetMethod("AsPointer").GetMethodBody().GetILAsByteArray();

			var ilHeader = csUnsafe
			              .GetMethod("AsPointer")
			              .GetMethodDesc()
			              .Reference
			              .GetILHeader();


			const string neko = "nyaa";

			int i    = 0;
			var reff = __makeref(i);
			__refvalue(__makeref(i), int) = 0;


			Debug.Assert(add(1, 1) == 2);

			Pointer<byte> stack = &i;
			Console.WriteLine(stack.Query());

			Console.WriteLine("{0:P}", Mem.StackBase);
			Console.WriteLine("{0:P}", Mem.StackLimit);

			Console.WriteLine(typeof(string).GetMetaType());
			
			// SHUT IT DOWN
			Clr.Close();
			Global.Close();
		}

		//This bypasses the restriction that you can't have a pointer to T,
		//letting you write very high-performance generic code.
		//It's dangerous if you don't know what you're doing, but very worth if you do.
		static T Read<T>(IntPtr address)
		{
			var obj = default(T);
			var tr  = __makeref(obj);

			//This is equivalent to shooting yourself in the foot
			//but it's the only high-perf solution in some cases
			//it sets the first field of the TypedReference (which is a pointer)
			//to the address you give it, then it dereferences the value.
			//Better be 10000% sure that your type T is unmanaged/blittable...
			unsafe {
				*(IntPtr*) (&tr) = address;
			}

			return __refvalue(tr, T);
		}

		static T add<T>(T a, T b)
		{
			if (a is int && b is int) {
				var c = __refvalue(__makeref(a), int);
				c += __refvalue(__makeref(b), int);
				return __refvalue(__makeref(c), T);
			}

			return default;
		}

		static void foo<T>(ref T value)
		{
			//This is the ONLY way to treat value as int, without boxing/unboxing objects
			if (value is int) {
				__refvalue(__makeref(value), int) = 1;
			}
			else {
				value = default(T);
			}
		}


		private static OpCode[] GetAllOpCodes()
		{
			var opCodeType   = typeof(OpCodes);
			var opCodeFields = opCodeType.GetFields(BindingFlags.Public | BindingFlags.Static);

			OpCode[] rgOpCodes = new OpCode[opCodeFields.Length];
			for (int i = 0;
				i < rgOpCodes.Length;
				i++) {
				rgOpCodes[i] = (OpCode) opCodeFields[i].GetValue(null);
			}

			return rgOpCodes;
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