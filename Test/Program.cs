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
using RazorSharp.Native.Enums;
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

		static bool asBool(int intValue)
		{
			bool boolValue = intValue != 0;
			return boolValue;
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

		[StructLayout(LayoutKind.Explicit)]
		public struct CharInfo
		{
			[FieldOffset(0)]
			internal char UnicodeChar;

			[FieldOffset(0)]
			internal char AsciiChar;

			[FieldOffset(2)]
			internal ushort Attributes;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct Coord
		{
			public short X;
			public short Y;

			public Coord(short x, short y)
			{
				X = x;
				Y = y;
			}
		};

		[StructLayout(LayoutKind.Sequential)]
		public struct Rect
		{
			public short Left;
			public short Top;
			public short Right;
			public short Bottom;
		}

		[DllImport("kernel32.dll")]
		static extern IntPtr GetConsoleWindow();

		[DllImport("kernel32.dll")]
		static extern uint GetLastError();

		[DllImport("kernel32.dll", SetLastError = true)]
		static extern bool WriteConsoleOutput(IntPtr   consoleHandle, CharInfo[,] buffer, Coord bufSize, Coord bufZero,
		                                      ref Rect drawRect);


		public enum CharAttributes : ushort
		{
			/// <summary>
			/// None.
			/// </summary>
			None = 0x0000,

			/// <summary>
			/// Text color contains blue.
			/// </summary>
			FOREGROUND_BLUE = 0x0001,

			/// <summary>
			/// Text color contains green.
			/// </summary>
			FOREGROUND_GREEN = 0x0002,

			/// <summary>
			/// Text color contains red.
			/// </summary>
			FOREGROUND_RED = 0x0004,

			/// <summary>
			/// Text color is intensified.
			/// </summary>
			FOREGROUND_INTENSITY = 0x0008,

			/// <summary>
			/// Background color contains blue.
			/// </summary>
			BACKGROUND_BLUE = 0x0010,

			/// <summary>
			/// Background color contains green.
			/// </summary>
			BACKGROUND_GREEN = 0x0020,

			/// <summary>
			/// Background color contains red.
			/// </summary>
			BACKGROUND_RED = 0x0040,

			/// <summary>
			/// Background color is intensified.
			/// </summary>
			BACKGROUND_INTENSITY = 0x0080,

			/// <summary>
			/// Leading byte.
			/// </summary>
			COMMON_LVB_LEADING_BYTE = 0x0100,

			/// <summary>
			/// Trailing byte.
			/// </summary>
			COMMON_LVB_TRAILING_BYTE = 0x0200,

			/// <summary>
			/// Top horizontal
			/// </summary>
			COMMON_LVB_GRID_HORIZONTAL = 0x0400,

			/// <summary>
			/// Left vertical.
			/// </summary>
			COMMON_LVB_GRID_LVERTICAL = 0x0800,

			/// <summary>
			/// Right vertical.
			/// </summary>
			COMMON_LVB_GRID_RVERTICAL = 0x1000,

			/// <summary>
			/// Reverse foreground and background attribute.
			/// </summary>
			COMMON_LVB_REVERSE_VIDEO = 0x4000,

			/// <summary>
			/// Underscore.
			/// </summary>
			COMMON_LVB_UNDERSCORE = 0x8000,
		}


		[HandleProcessCorruptedStateExceptions]
		public static void Main(string[] args)
		{
			Global.Setup();
			Clr.ClrPdb = new FileInfo(@"C:\Symbols\clr.pdb");
			Clr.Setup();

			

			var rect  = new Rect() {Left = 10, Top = 0, Right = 11, Bottom = 1};
			var bufsz = new Coord(2, 2);
			var buf   = new CharInfo[2, 2];
			buf[0, 0] = new CharInfo {AsciiChar = '+'};
			var bufpos = new Coord(0, 0);


			string frame = "SunAwtFrame";
			var    hnd   = new IntPtr(0x00810BA2);
			var    hnd2  = User32.FindWindow(frame, null);
			Console.WriteLine(">> {0}", Hex.ToHex(hnd2));
			User32.GetWindowThreadProcessId(hnd2, out var pid);
			Console.WriteLine(pid);
			var proc = Process.GetProcessById((int) pid);



			var method = typeof(Program).GetAnyMethod("doSomething");
			var method2 = typeof(Program).GetAnyMethod("doSomething2");

			var ss=SigScanner.QuickScan("clr.dll", "48 89 5C 24 10 48 89 74 24 18 57 48 83");
			Console.WriteLine("ss: {0:X}",ss.ToInt64());
			
			var sym=Symbols.GetSymAddress(Clr.ClrPdb.FullName, "clr.dll", "MethodDesc::SetStableEntryPointInterlocked");
			Console.WriteLine("sym: {0}", sym);
			
			const int offset = 0x1A9418;
			var seg = Segments.GetSegment(".text", "clr.dll");
			var ptr2 = (seg.SectionAddress + offset);
			Console.WriteLine("fn @ {0:P}",ptr2);
			Console.WriteLine(ptr2.Query());
			Console.WriteLine(Collections.CreateString(ptr2.CopyOutBytes(5), ToStringOptions.Hex));
			var fnp=Marshal.GetDelegateForFunctionPointer<ClrFunctions.SetStableEntryPointInterlockedDelegate>(ptr2.Address);


			var md = (MethodDesc*) method.MethodHandle.Value;
			var ep = (ulong) method2.MethodHandle.GetFunctionPointer();

			var res = fnp(md, ep);
			Console.WriteLine("!!>> {0}",res);
			
			
			
			Console.WriteLine(typeof(string).GetMetaType());
			var fn=ClrFunctions.GetClrFunctionAddress("GetThreadGeneric");
			Console.WriteLine("bro");
			Console.WriteLine("{0:P}",fn);
			
			

			var fnOffset = 0x4180;
			
			// SHUT IT DOWN
			Clr.Close();
			Global.Close();
			
		}

		static void doSomething2()
		{
			Console.WriteLine("doSomething2");
		}
		
		static void doSomething()
		{
			Console.WriteLine("doSomething");
		}
		
		delegate void* get(void* x, void* y);
		
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