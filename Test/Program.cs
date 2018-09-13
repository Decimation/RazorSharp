#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using JetBrains.Annotations;
using RazorCommon;
using RazorCommon.Extensions;
using RazorCommon.Strings;
using RazorInvoke;
using RazorInvoke.Libraries;
using RazorSharp;
using RazorSharp.Analysis;
using RazorSharp.CLR;
using RazorSharp.CLR.Fixed;
using RazorSharp.CLR.Structures;
using RazorSharp.CLR.Structures.HeapObjects;
using RazorSharp.Memory;
using RazorSharp.Pointers;
using RazorSharp.Utilities;
using RazorSharp.Utilities.Exceptions;
using Test.Testing;
using Test.Testing.Benchmarking;
using Test.Testing.Types;
using static RazorSharp.Unsafe;
using Module = System.Reflection.Module;
using Point = Test.Testing.Types.Point;

#endregion

namespace Test
{

	#region

	using Unsafe = RazorSharp.Unsafe;
	using CSUnsafe = System.Runtime.CompilerServices.Unsafe;

	#endregion

	/**
	 * RazorSharp
	 *
	 * History:
	 * 	- RazorSharp (deci-common-c)
	 * 	- RazorSharpNeue
	 * 	- RazorCLR
	 * 	- RazorSharp
	 *
	 * Notes:
	 *  - 32-bit is not fully supported
	 *  - Most types are probably not thread-safe
	 *
	 * Goals:
	 *  - Provide identical functionality of ClrMD, SOS, and Reflection
	 * 	  but in a faster and more efficient way
	 */
	internal static unsafe class Program
	{


#if DEBUG
		static Program()
		{
			StandardOut.ModConsole();

			/**
			 * RazorSharp is tested on and targets:
			 *
			 * - x64
			 * - Windows
			 * - .NET CLR 4.7.2
			 * - Workstation Concurrent GC
			 *
			 *
			 */
			RazorContract.Assert(IntPtr.Size == 8);
			RazorContract.Assert(Environment.Is64BitProcess);
			RazorContract.Assert(RuntimeInformation.IsOSPlatform(OSPlatform.Windows));

			/**
			 * 4.0.30319.42000
			 * The version we've been testing and targeting.
			 * Other versions will probably work but we're just making sure
			 * todo - determine compatibility
			 */
			RazorContract.Assert(Environment.Version.Major == 4);
			RazorContract.Assert(Environment.Version.Minor == 0);
			RazorContract.Assert(Environment.Version.Build == 30319);
			RazorContract.Assert(Environment.Version.Revision == 42000);

			RazorContract.Assert(!GCSettings.IsServerGC);
			bool isRunningOnMono = Type.GetType("Mono.Runtime") != null;
			RazorContract.Assert(!isRunningOnMono);

//			Logger.Log(Flags.Info, "Architecture: x64");
//			Logger.Log(Flags.Info, "Byte order: {0}", BitConverter.IsLittleEndian ? "Little Endian" : "Big Endian");
//			Logger.Log(Flags.Info, "CLR {0}", Environment.Version);
		}
#endif

		// todo: protect address-sensitive functions


		/**
		 * >> Entry point
		 */
		public static void Main(string[] args)
		{
/*
			// Method Address = Method Virtual Address + base address of class that declares this member..
			SignatureCall.DynamicBind(typeof(Program), "setObjRef");
			Pointer<char> cstr = stackalloc char[3];
			Console.WriteLine(cstr.ToTable(3).ToMarkDownString());
			cstr.Init("foo");
			Console.WriteLine(cstr.ToTable(3).ToMarkDownString());


			Debug.Assert(cstr.ToString() == "foo");
			Debug.Assert(cstr.SequenceEqual("foo"));


			byte* b = stackalloc byte[BaseInstanceSize<CPoint>()];
			Mem.StackInit<CPoint>(ref b);
			Pointer<CPoint> lpMem = &b;
			Console.WriteLine(InspectorHelper.LayoutString(ref lpMem.Reference));


			var ptr = lpMem.Read<Pointer<byte>>();
//			ptr.Write("foo", 1);

			Console.WriteLine(GCHeap.GlobalHeap.Reference.GCCount);
			__break();
			GC.Collect();
			Console.WriteLine(GCHeap.GlobalHeap.Reference.GCCount);
			Console.WriteLine(lpMem);

			__break();
*/
			object o = new object();
			InspectorHelper.Inspect(ref o, InspectorMode.Address);
			__break();
			Console.WriteLine(GCHeap.GlobalHeap.Reference.GCCount);
			GC.Collect();
			__break();
			Console.WriteLine(GCHeap.GlobalHeap.Reference.GCCount);
		}




		[CLRSigcall("48 89 11 E9 10 CA ED FF")]
		static void setObjRef(void** dst, void* src) { }


		static TTo reinterpret_cast<TFrom, TTo>(TFrom tf)
		{
			return CSUnsafe.Read<TTo>(Unsafe.AddressOf(ref tf).ToPointer());
		}


		private static class WinDbg
		{
			public static void DumpObj<T>(ref T t)
			{
				Console.WriteLine(DumpObjInfo.Get(ref t));
			}

			private struct DumpObjInfo
			{
				private readonly string               m_szName;
				private readonly Pointer<MethodTable> m_pMT;
				private readonly Pointer<EEClass>     m_pEEClass;
				private readonly int                  m_cbSize;
				private readonly string               m_szStringValue;
				private readonly Pointer<FieldDesc>[] m_rgpFieldDescs;

				private ConsoleTable m_fieldTable;


				public static DumpObjInfo Get<T>(ref T t)
				{
					var mt = Runtime.ReadMethodTable(ref t);
					var sz = t is string s ? s : "-";

					var dump = new DumpObjInfo(mt.Reference.Name, mt, mt.Reference.EEClass, AutoSizeOf(in t), sz,
						Runtime.GetFieldDescs<T>());
					dump.m_fieldTable = dump.FieldsTable(ref t);

					return dump;
				}


				private DumpObjInfo(string szName, Pointer<MethodTable> pMt, Pointer<EEClass> pEEClass, int cbSize,
					string szStringValue, Pointer<FieldDesc>[] rgpFieldDescs)
				{
					m_szName        = szName;
					m_pMT           = pMt;
					m_pEEClass      = pEEClass;
					m_cbSize        = cbSize;
					m_szStringValue = szStringValue;
					m_rgpFieldDescs = rgpFieldDescs;
					m_fieldTable    = null;
				}

				private ConsoleTable FieldsTable<T>(ref T t)
				{
					// A few differences:
					// - FieldInfo.Attributes is used for the Attr column; I don't know what WinDbg uses
					var table = new ConsoleTable("MT", "Field", "Offset", "Type", "VT", "Attr", "Value", "Name");
					foreach (var v in m_rgpFieldDescs) {
						table.AddRow(Hex.ToHex(v.Reference.TypeMethodTable.Address), Hex.ToHex(v.Reference.MemberDef),
							v.Reference.Offset,
							v.Reference.Info.FieldType.Name, v.Reference.Info.FieldType.IsValueType,
							v.Reference.Info.Attributes, v.Reference.GetValue(t),
							v.Reference.Name);
					}

					return table;
				}

				public override string ToString()
				{
					var table = new ConsoleTable("Attribute", "Value");
					table.AddRow("Name", m_szName);
					table.AddRow("MethodTable", Hex.ToHex(m_pMT.Address));
					table.AddRow("EEClass", Hex.ToHex(m_pEEClass.Address));
					table.AddRow("Size", String.Format("{0} ({1}) bytes", m_cbSize, Hex.ToHex(m_cbSize)));
					table.AddRow("String", m_szStringValue);

					return String.Format("{0}\nFields:\n{1}", table.ToMarkDownString(),
						m_fieldTable.ToMarkDownString());
				}
			}
		}


		class CPoint
		{
			private float m_fX,
			              m_fY;


			public string String { get; set; }


			public float X {
				get => m_fX;
				set => m_fX = value;
			}

			public float Y {
				get => m_fY;
				set => m_fY = value;
			}

			public override string ToString()
			{
				return String.Format("x = {0}, y = {1}, String = {2}", m_fX, m_fY, String);
			}
		}


		private static long ReadBitsAsInt64(ulong u, int bitIndex, int bits)
		{
			long value = 0;

			// Calculate a bitmask of the desired length
			long mask = 0;
			for (int i = 0; i < bits; i++)
				mask |= 1 << i;

			value |= ((uint) u & mask) << bitIndex;
			return value;
		}


		private static void RunBenchmark<T>()
		{
			BenchmarkRunner.Run<T>();
		}


		private static void __break()
		{
			Console.ReadLine();
		}

		#region todo

		private static void VMMap()
		{
			var table = new ConsoleTable("Low address", "High address", "Size");

			// Stack of current thread
			table.AddRow(Hex.ToHex(Mem.StackLimit), Hex.ToHex(Mem.StackBase),
				String.Format("{0} ({1} K)", Mem.StackSize, Mem.StackSize / Mem.BytesInKilobyte));
			Console.WriteLine(InspectorHelper.CreateLabelString("Stack:", table));

			table.Rows.RemoveAt(0);

			// GC heap
			table.AddRow(Hex.ToHex(GCHeap.LowestAddress), Hex.ToHex(GCHeap.HighestAddress),
				String.Format("{0} ({1} K)", GCHeap.Size, GCHeap.Size / Mem.BytesInKilobyte));
			Console.WriteLine(InspectorHelper.CreateLabelString("GC:", table));
		}

		private static T* AddrOf<T>(ref T t) where T : unmanaged
		{
			return (T*) AddressOf(ref t);
		}

		private static void Hook<TOrig, TNew>(string origFn, string newFn)
		{
			Hook(typeof(TOrig), origFn, typeof(TNew), newFn);
		}

		private static void Hook(Type tOrig, string origFn, Type tNew, string newFn)
		{
			Pointer<MethodDesc> origMd = Runtime.GetMethodDesc(tOrig, origFn);
			Pointer<MethodDesc> newMd  = Runtime.GetMethodDesc(tNew, newFn);
			origMd.Reference.SetFunctionPointer(newMd.Reference.Function);
		}

		#endregion


		/**
		 * Dependencies:
		 *
		 * RazorSharp:
		 *  - RazorCommon
		 * 	- CompilerServices.Unsafe
		 *  - RazorInvoke
		 *
		 * Test:
		 *  - RazorCommon
		 *  - CompilerServices.Unsafe
		 *  - RazorInvoke
		 * 	- NUnit
		 *  - BenchmarkDotNet
		 */

		/**
		 * Class this ptr:
		 *
		 * public IntPtr __this {
		 *		get {
		 *			var v = this;
		 *			var hThis = Unsafe.AddressOfHeap(ref v);
		 *			return hThis;
		 *		}
		 *	}
		 *
		 *
		 * Struct this ptr:
		 *
		 * public IntPtr __this {
		 *		get => Unsafe.AddressOf(ref this);
		 * }
		 */

		/**
		 * CLR										Used in										Equals
		 *
		 * MethodTable.BaseSize						Unsafe.BaseInstanceSize, Unsafe.HeapSize	-
		 * MethodTable.ComponentSize				Unsafe.HeapSize								-
		 * MethodTable.NumInstanceFieldBytes		Unsafe.BaseFieldsSize						-
		 * EEClass.m_cbNativeSize					Unsafe.NativeSize							Marshal.SizeOf, EEClassLayoutInfo.m_cbNativeSize
		 * EEClassLayoutInfo.m_cbNativeSize			-											Marshal.SizeOf, EEClass.m_cbNativeSize
		 * EEClassLayoutInfo.m_cbManagedSize		-											Unsafe.SizeOf, Unsafe.BaseFieldsSize (value types)
		 */


		/**
		 * #defines:
		 *
		 * FEATURE_COMINTEROP
		 * _TARGET_64BIT_
		 */

	}

}