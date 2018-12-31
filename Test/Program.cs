#region

//using Microsoft.Diagnostics.Runtime;

#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Running;
using Newtonsoft.Json;
using RazorSharp;
using RazorSharp.Analysis;
using RazorSharp.CLR;
using RazorSharp.CLR.Meta;
using RazorSharp.CLR.Structures;
using RazorSharp.CLR.Structures.EE;
using RazorSharp.Common;
using RazorSharp.Memory;
using RazorSharp.Pointers;
using RazorSharp.Utilities;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Test.Testing.Benchmarking;
using Test.Testing.Tests.Metadata;
using static RazorSharp.Unsafe;
using Unsafe = RazorSharp.Unsafe;

#endregion

// ReSharper disable InconsistentNaming

#endregion

namespace Test
{

	#region

	using DWORD = UInt32;
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
	 *  - Provide identical and better functionality of ClrMD, SOS, and Reflection
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
		// todo: replace native pointers* with Pointer<T> for consistency
		// todo: RazorSharp, ClrMD, Reflection comparison
		// todo: Contract-oriented programming
		// todo: read module memory

		private static Pointer<byte> virtaddr(string module, long ofs)
		{
			// 0x18fd1c
			Pointer<byte> ptr = Modules.GetModule(module).BaseAddress;
			return ptr.Add(ofs);
		}

		public static void Main(string[] args)
		{
			/*string a = "nil";
			string b = "nil";

			var ptr  = Unsafe.AddressOf(ref a);
			var ptrx = Unsafe.AddressOf(ref b);

			Console.WriteLine(ptr);
			Console.WriteLine(ptrx);

			var s = ptr.ReadAny<Pointer<char>>().AddressOfIndex(RuntimeHelpers.OffsetToStringData / 2);
			Console.WriteLine(s);
			s[0] = 'g';
			Debug.Assert(a == b);
			Debug.Assert("gil" == a);
			s.WriteAll("gamer");
			Debug.Assert(s.ReadAny<int>(-1) == 3);
			s.WriteAny(5, -1);
			Debug.Assert(a == "gamer");


			var levelSwitch = new LoggingLevelSwitch();
			var log = new LoggerConfiguration()
				.MinimumLevel.ControlledBy(levelSwitch)
				.WriteTo.ColoredConsole()
				.CreateLogger();

			log.Information("i");
			log.Information(Environment.Version.ToString());*/

			Console.WriteLine("-> {0}", Assembly.Load("RazorSharp").GetTypes().First(t => t.Name == "FieldDesc"));
			string f = Environment.GetEnvironmentVariable("userprofile")
			           + "\\Desktop\\ClrFunctions.json";

			Console.WriteLine(f);

			//dynamic data = JsonConvert.DeserializeObject<SignatureCall.Root>(File.ReadAllText(	));


			ClrMetaTests.MethodTable();
			ClrMetaTests.FieldDesc();
			ClrMetaTests.MethodDesc();
			ClrMetaTests.GC();
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


		private static int AddOp(int a, int b)
		{
			return a + b;
		}


		public static bool IsInterned(this string text)
		{
			return string.IsInterned(text) != null;
		}


		//struct CORINFO_METHOD_INFO
		//{
		//	CORINFO_METHOD_HANDLE ftn;
		//	CORINFO_MODULE_HANDLE scope;
		//	BYTE *                ILCode;
		//	unsigned              ILCodeSize;
		//	unsigned              maxStack;
		//	unsigned              EHcount;
		//	CorInfoOptions        options;
		//	CorInfoRegionKind     regionKind;
		//	CORINFO_SIG_INFO      args;
		//	CORINFO_SIG_INFO      locals;
		//};


		#region todo

		/*static void Region()
		{
			var table   = new ConsoleTable("Region", "Address", "Size", "GC Segment", "GC Heap");
			var regions = GetRuntime().EnumerateMemoryRegions().OrderBy(x => x.Address).DistinctBy(x => x.Address);

			foreach (var region in regions) {
				table.AddRow(region.Type, Hex.ToHex(region.Address), region.Size,
					region.Type == ClrMemoryRegionType.GCSegment ? region.GCSegmentType.ToString() : "-",
					region.HeapNumber != -1 ? region.HeapNumber.ToString() : "-");
			}

			Console.WriteLine(table.ToMarkDownString());
		}

		static string AutoCreateFieldTable<T>(ref T t)
		{
			var table  = new ConsoleTable("Field", "Value");
			var fields = Runtime.GetFieldDescs<T>();
			foreach (var f in fields) {
				if (f.Reference.IsPointer)
					table.AddRow(f.Reference.Name, ReflectionUtil.GetPointerForPointerField(f, ref t).ToString("P"));
				else table.AddRow(f.Reference.Name, f.Reference.GetValue(t));
			}

			return table.ToMarkDownString();
		}

		private static ClrRuntime GetRuntime()
		{
			var dataTarget =
				DataTarget.AttachToProcess(Process.GetCurrentProcess().Id, UInt32.MaxValue, AttachFlag.Passive);
			return dataTarget.ClrVersions.Single().CreateRuntime();
		}*/


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
				private          ConsoleTable         m_fieldTable;

				public static DumpObjInfo Get<T>(ref T t)
				{
					Pointer<MethodTable> mt = Runtime.ReadMethodTable(ref t);
					string               sz = t is string s ? s : "-";

					DumpObjInfo dump = new DumpObjInfo(mt.Reference.Name, mt, mt.Reference.EEClass, AutoSizeOf(t), sz,
						typeof(T).GetFieldDescs());
					dump.m_fieldTable = dump.FieldsTable(ref t);

					return dump;
				}


				private DumpObjInfo(string szName, Pointer<MethodTable> pMt, Pointer<EEClass> pEEClass,
					int cbSize, string szStringValue, Pointer<FieldDesc>[] rgpFieldDescs)
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
					ConsoleTable table =
						new ConsoleTable("MT", "Field", "Offset", "Type", "VT", "Attr", "Value", "Name");
					foreach (Pointer<FieldDesc> v in m_rgpFieldDescs) {
						table.AddRow(
							Hex.ToHex(v.Reference.FieldMethodTable.Address),
							Hex.ToHex(v.Reference.Token),
							v.Reference.Offset,
							v.Reference.Info.FieldType.Name, v.Reference.Info.FieldType.IsValueType,
							v.Reference.Info.Attributes, v.Reference.GetValue(t),
							v.Reference.Name);
					}

					return table;
				}

				public override string ToString()
				{
					ConsoleTable table = new ConsoleTable("Attribute", "Value");
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


		private static void RunBenchmark<T>()
		{
			BenchmarkRunner.Run<T>(new AllowNonOptimized());
		}

		private static void VmMap()
		{
			ConsoleTable table = new ConsoleTable("Low address", "High address", "Size");

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

		#endregion

// @formatter:off — disable formatter after this line
// @formatter:on — enable formatter after this line


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