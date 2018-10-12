#region

//using Microsoft.Diagnostics.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Running;
using JetBrains.Annotations;
using NUnit.Framework;
using RazorSharp;
using RazorSharp.Analysis;
using RazorSharp.CLR;
using RazorSharp.CLR.Fixed;
using RazorSharp.CLR.Structures;
using RazorSharp.CLR.Structures.EE;
using RazorSharp.CLR.Structures.ILMethods;
using RazorSharp.Common;
using RazorSharp.Memory;
using RazorSharp.Native;
using RazorSharp.Native.Enums;
using RazorSharp.Native.Structures;
using RazorSharp.Pointers;
using RazorSharp.Utilities;
using Test.Testing.Benchmarking;
using Test.Testing.Types;
using static RazorSharp.Unsafe;
using ProcessorArchitecture = System.Reflection.ProcessorArchitecture;
using Unsafe = RazorSharp.Unsafe;

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

		/**
		 * >> Entry point
		 */
		public static void Main(string[] args)
		{
			// todo: read module memory
			/*
//			Target tgt = new Target();

			var mdAdd = Runtime.GetMethodDesc<Target>("add");
			Debug.Assert(!mdAdd.Reference.IsPointingToNativeCode);
			mdAdd.Reference.Prepare();
			Debug.Assert(mdAdd.Reference.IsPointingToNativeCode);

			Runtime.GetMethodDesc(typeof(Program), "Val").Reference.Prepare();

			var nativeCode = AddressOfFunction(typeof(Program), "Val");

			Console.WriteLine(Collections.ToString(nativeCode.CopyOut(0x3E + 1)));

			Console.WriteLine(Val(1, 2));

			nativeCode[0x27] = 0x2B;

			Console.WriteLine(Val(1, 2));
			Console.WriteLine(nativeCode.Query());


			// xrefs to ?g_jitHost@@3PEAVICorJitHost@@EA at 0x18010b970 from:

			SignatureCall.DynamicBind(typeof(Program));

			// nativeSizeOfCode

			*/


			//000000AFB6DFE990

			//000000EBEBBFFCC8
			long l = 0;


			Console.WriteLine("Stack base = {0}", Hex.ToHex(Mem.StackBase));
			Console.WriteLine("Stack lim = {0}", Hex.ToHex(Mem.StackLimit));

			Console.WriteLine("&l = {0}", Hex.ToHex(&l));

//			var rdi = PointerUtils.Add(&l, 0x10);
			//		Console.WriteLine("rdi = {0}", Hex.ToHex(rdi));

			// or rbp?
			// sometimes wrong - frick
			var rsp = PointerUtils.Subtract(&l, 0xE0).Subtract(0x20).Subtract(0x10).Subtract(0x10);
			Console.WriteLine("rsp = {0}", Hex.ToHex(rsp));


			Console.WriteLine("{0:P}", get_reg());
			Console.WriteLine("Frame ptr = {0}", Hex.ToHex(&l - 1));
			Console.WriteLine("Ret addr = {0}", Hex.ToHex(&l - 2));


			string sz = "jew";

			AddressOf(ref sz).WriteAs<string, long>("bar");
			Debug.Assert(sz == "bar");
			AddressOf(ref sz).WriteAs<string, IntPtr>("baz");
			Debug.Assert(sz == "baz");
			AddressOfHeap(ref sz, OffsetType.StringData).Write('j', 2);
			Debug.Assert(sz == "baj");


			float pi    = 3.14F;
			var   piptr = Unsafe.AddressOf(ref pi);
			Debug.Assert(piptr.ReadAs<float, int>() == *((int*) &pi));

			int i = *((int*) &pi);
			Debug.Assert(Unsafe.AddressOf(ref i).ReadAs<int, float>() == 3.14F);


			var       alloc = Mem.AllocUnmanaged<Structure>();
			Structure s     = new Structure
			{
				s = "joo"
			};
			alloc.Write(s);
			Console.WriteLine(alloc);


			Mem.Free(alloc);
		}

		struct Structure
		{
			public string s;

			public override string ToString()
			{
				return String.Format("[{0}]", s);
			}
		}

		static void Interpret<T>(T t) { }

		static bool _bool(int i)
		{
			return i > 0;
		}


		private static int get()
		{
			return -1;
		}

		static void overwrite()
		{
			byte* buffer = stackalloc byte[10];
			gets(buffer);
		}

		static void gets(byte* buffer)
		{
			string val = Console.ReadLine();
			Mem.Copy(buffer, Encoding.UTF8.GetBytes(val));
		}

		static Pointer<byte> get_reg()
		{
			long          l;
			Pointer<byte> rbp = &l + 1;
			return rbp;
		}


		static void auto(Pointer<Pointer<int>> lpInt32)
		{
			int z = 0xFFFFFF;
			lpInt32.Reference = &z;
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

		[Sigcall(Module = "clrjit.dll", Signature = "48 8B 05 B9 D8 05 00 48 85 C0 75 1C 48 8D 05 CD 34 04 00")]
		private static Pointer<byte> GetJit()
		{
			return null;
		}

		private static int Val(int a, int b)
		{
			a += b;
			return a;
		}


		static TDelegate HyperInvoke<TType, TDelegate>(string name) where TDelegate : Delegate
		{
			return Runtime.GetMethodDesc<TType>(name).Reference.GetDelegate<TDelegate>();
		}


		private static class Dbg
		{
			[Conditional("DEBUG")]
			public static void Break()
			{
				Console.ReadLine();
			}
		}


		[DllImport("kernel32.dll", EntryPoint = "RtlCopyMemory")]
		private static extern void CopyMemory(void* dest, void* src, uint len);

		private delegate int add(Target ptr, int i, int j);


		class Hyper
		{
			public Hyper()
			{
				Swap<object>();
			}

			public bool IsType<T>()
			{
				return GetType() == typeof(T);
			}

			public void Swap<T>()
			{
				var heap = Unsafe.AddressOfHeap(this);
				heap.Write(Runtime.MethodTableOf<T>());
				Debug.Assert(IsType<T>());
			}

			public void Write<T>(T t, int elemOffset = 0)
			{
				var heap = Unsafe.AddressOfHeap(this, OffsetType.Fields);


				heap.Write(t, elemOffset);
			}
		}


		[NotNull]
		public static List<MemoryBasicInformation> GetRegionsOfStack()
		{
			GetStackExtents(out byte* stackBase, out long stackSize);

			List<MemoryBasicInformation> result = new List<MemoryBasicInformation>();

			byte* current = stackBase;
			while (current < stackBase + stackSize) {
				MemoryBasicInformation info = new MemoryBasicInformation();
				Kernel32.VirtualQuery(new IntPtr(current), ref info, (uint) sizeof(MemoryBasicInformation));
				result.Add(info);
				current = (byte*) PointerUtils.Add(info.BaseAddress, info.RegionSize);
			}

			result.Reverse();
			return result;
		}


		public static void GetStackExtents(out byte* stackBase, out long stackSize)
		{
			MemoryBasicInformation info = new MemoryBasicInformation();
			Kernel32.VirtualQuery(new IntPtr(&info), ref info, (uint) sizeof(MemoryBasicInformation));
			stackBase = (byte*) info.AllocationBase;
			stackSize = ((PointerUtils.Subtract(info.BaseAddress, info.AllocationBase).ToInt64()) +
			             info.RegionSize.ToInt64());
		}


		private static void dmp<T>(ref T t) where T : class
		{
			ConsoleTable table = new ConsoleTable("Info", "Value");
			table.AddRow("Stack", Hex.ToHex(AddressOf(ref t).Address));
			table.AddRow("Heap", Hex.ToHex(AddressOfHeap(ref t).Address));
			table.AddRow("Size", AutoSizeOf(t));

			Console.WriteLine(table.ToMarkDownString());
		}


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

		private static TTo reinterpret_cast<TFrom, TTo>(TFrom tf)
		{
			return CSUnsafe.Read<TTo>(AddressOf(ref tf).ToPointer());
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
				private          ConsoleTable         m_fieldTable;

				public static DumpObjInfo Get<T>(ref T t)
				{
					Pointer<MethodTable> mt = Runtime.ReadMethodTable(ref t);
					string               sz = t is string s ? s : "-";

					DumpObjInfo dump = new DumpObjInfo(mt.Reference.Name, mt, mt.Reference.EEClass, AutoSizeOf(t), sz,
						Runtime.GetFieldDescs<T>());
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
			BenchmarkRunner.Run<T>();
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

		private static T* AddrOf<T>(ref T t) where T : unmanaged
		{
			return (T*) AddressOf(ref t);
		}

		#endregion

// @formatter:off — disable formatter after this line
// @formatter:on — enable formatter after this line

		/**
		 * Dependencies:
		 *
		 * RazorSharp:
		 * 	- CompilerServices.Unsafe
		 *
		 * Test:
		 *  - CompilerServices.Unsafe
		 * 	- NUnit
		 *  - BenchmarkDotNet
		 *  - ClrMD
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