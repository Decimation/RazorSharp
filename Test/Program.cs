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


		private static object Proxy(long l)
		{
			return MemConvert.ProxyCast<long, object>(l);
		}

		private delegate void* Alloc(GCHeap* __this, uint size, uint flags);
		private delegate void* AllocateObject(MethodTable* mt, int boolValue);
		
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
			var cpy=ptr.Value;

			var textSeg = Segments.GetSegment(".text", Clr.CLR_DLL_SHORT);
			Console.WriteLine(textSeg);
			Pointer<byte> segAddr = textSeg.SectionAddress;
			
			// .text:0000000180001000
			// .text:000000018058E880

			var fnOffset = 0x000000018058E880 - 0x0000000180001000;
			Console.WriteLine("offset {0}", fnOffset);

			var fnAddr = segAddr + fnOffset;
			Console.WriteLine("fn addr {0}", fnAddr);

//			var allocFn = Marshal.GetDelegateForFunctionPointer<Alloc>(fnAddr.Address);
//			var ptrAlloc = allocFn(GCHeap.GlobalHeap.ToPointer<GCHeap>(), 10, 0);
//			Console.WriteLine(Hex.ToHex(ptrAlloc));

			var allocObj    = ClrFunctions.GetClrFunctionAddress<AllocateObject>("AllocateObject");
			var objValuePtr = allocObj(typeof(List<int>).GetMethodTable().ToPointer<MethodTable>(), 0);
			Console.WriteLine(Hex.ToHex(objValuePtr));
			var listNative = CSUnsafe.Read<List<int>>(&objValuePtr);
			Console.WriteLine(listNative);
			
			GC.Collect();

			var sz = GCHeap.AllocateObject<string>(0);
			
			
			//var fn = ClrFunctions.GetClrFunctionAddressSig<Alloc>(
			//	"56 57 41 41 56 41 57 48 83 EC 30 48 C7 44 24 20 FE FF FF FF 48 89 5C 24 60 48 89 6C 24 70 45 8B E0 4C 8B F2 E9 00 01 00 00");
			//var ptrAlloc=fn(GCHeap.GlobalHeap.ToPointer<GCHeap>(),0,0);
			
			
			
			const string str = "foo";
			var sptr = memcpy(str);
			Console.WriteLine(sptr);
			freecpy(sptr);


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