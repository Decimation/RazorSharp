﻿#region

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
using Microsoft.Diagnostics.Runtime;
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
using Test.Testing.Tests;
using Test.Testing.Types;
using static RazorSharp.Unsafe;
using Module = System.Reflection.Module;
using Point = Test.Testing.Types.Point;
using Runtime = RazorSharp.CLR.Runtime;

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
		// todo: replace native pointers* with Pointer<T> for consistency


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


//			VMMap();
//			regions();

			var       pMd       = Runtime.GetMethodDesc<CPoint>("getInt32");
			ClrMethod clrMethod = GetRuntime().GetMethodByHandle(pMd.ToUInt64());

			Console.WriteLine(clrMethod);
			Console.WriteLine(Hex.ToHex(pMd.Reference.Function));
			Console.WriteLine(Hex.ToHex(clrMethod.NativeCode));
			Console.WriteLine(Hex.ToHex(clrMethod.IL.Address));
			Console.WriteLine(clrMethod.IL.Length);
			Console.WriteLine(Collections.ToString(pMd.Reference.Info.GetMethodBody()?.GetILAsByteArray()));
			Console.WriteLine(Mem.Read<byte>(clrMethod.IL.Address, 1).ToString("X"));
		}

		interface ICLRDataTarget
		{
			int GetCurrentThreadID(out uint threadID);
			int GetImageBase(string imagePath, out ulong baseAddress);
			int GetMachineType(out ulong machineType);
			int GetPointerSize(out ulong pointerSize);
			int GetThreadContext(uint threadID, uint contextFlags, uint contextSize, out byte context);
			int GetTLSValue(uint threadID, uint index, out ulong value);
			int ReadVirtual(ulong address, out byte buffer, uint bytesRequested, out uint bytesRead);
			int Request(uint reqCode, uint inBufferSize, ref byte inBuffer, uint outBufferSize, out byte outBuffer);
			int SetThreadContext(uint threadID, uint contextSize, ref byte context);
			int SetTLSValue(uint threadID, uint index, ulong value);
			int WriteVirtual(ulong address, ref byte buffer, uint bytesRequested, out uint bytesWritten);
		}

		internal struct CodeHeaderData
		{
			public ulong GCInfo;
			public uint  JITType;
			public ulong MethodDescPtr;
			public ulong MethodStart;
			public uint  MethodSize;
			public ulong ColdRegionStart;
			public uint  ColdRegionSize;
			public uint  HotRegionSize;
		}

		internal struct V45MethodDescData
		{
			private uint  _bHasNativeCode;
			private uint  _bIsDynamic;
			private short _wSlotNumber;

			internal ulong NativeCodeAddr;

			// Useful for breaking when a method is jitted.
			private ulong _addressOfNativeCodeSlot;

			internal ulong MethodDescPtr;
			internal ulong MethodTablePtr;
			internal ulong ModulePtr;

			internal uint  MDToken;
			public   ulong GCInfo;
			private  ulong _GCStressCodeCopy;

			// This is only valid if bIsDynamic is true
			private ulong _managedDynamicMethodObject;

			private ulong _requestedIP;

			// Gives info for the single currently active version of a method
			private V45ReJitData _rejitDataCurrent;

			// Gives info corresponding to requestedIP (for !ip2md)
			private V45ReJitData _rejitDataRequested;

			// Total number of rejit versions that have been jitted
			private uint _cJittedRejitVersions;
		}

		internal struct V45ReJitData
		{
			private ulong _rejitID;
			private uint  _flags;
			private ulong _nativeCodeAddr;
		}


		[ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("436f00f2-b42a-4b9f-870c-e73db66ae930")]
		internal interface ISOSDac
		{
			[PreserveSig]
			int GetMethodDescData(ulong methodDesc, ulong ip, out V45MethodDescData data, uint cRevertedRejitVersions,
				V45ReJitData[] rgRevertedRejitData, out ulong pcNeededRevertedRejitData);
		}


		static void threads()
		{
			foreach (var thread in GetRuntime().Threads) {
				Console.WriteLine("{0} [{1} {2}]", Hex.ToHex(thread.Address), Hex.ToHex(thread.StackBase),
					Hex.ToHex(thread.StackLimit));

				foreach (var o in thread.EnumerateStackObjects()) {
					Console.WriteLine(o);
				}

				foreach (var stackFrame in thread.EnumerateStackTrace()) {
					Console.WriteLine(stackFrame.ModuleName);
				}
			}
		}

		static void regions()
		{
			var table = new ConsoleTable("Address", "Size", "Type", "Segment");

			foreach (var region in GetRuntime().EnumerateMemoryRegions()) {
				table.AddRow(Hex.ToHex(region.Address), region.Size, region.Type,
					region.Type == ClrMemoryRegionType.GCSegment ? region.GCSegmentType.ToString() : "-");
			}


			Console.WriteLine(table.ToMarkDownString());
		}


		static ClrRuntime GetRuntime()
		{
			var dataTarget =
				DataTarget.AttachToProcess(Process.GetCurrentProcess().Id, UInt32.MaxValue, AttachFlag.Passive);
			return dataTarget.ClrVersions.Single().CreateRuntime();
		}


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
						table.AddRow(Hex.ToHex(v.Reference.TypeMethodTable.Address), Hex.ToHex(v.Reference.Token),
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

			public int getInt32()
			{
				return 1;
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