#region

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using BenchmarkDotNet.Running;
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
using static RazorSharp.Unsafe;
using Module = System.Reflection.Module;

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
			 * RazorSharp is only compatible on:
			 *
			 * - x64
			 * - Windows
			 * - .NET CLR
			 * - Workstation GC
			 *
			 * Current target: .NET 4.7.2
			 *
			 */
			RazorContract.Assert(IntPtr.Size == 8);
			RazorContract.Assert(Environment.Is64BitProcess);
			RazorContract.Assert(RuntimeInformation.IsOSPlatform(OSPlatform.Windows));
			RazorContract.Assert(Environment.Version.Major == 4); //todo
			RazorContract.Assert(!GCSettings.IsServerGC);
			bool isRunningOnMono = Type.GetType("Mono.Runtime") != null;
			RazorContract.Assert(!isRunningOnMono);

//			Logger.Log(Flags.Info, "Architecture: x64");
//			Logger.Log(Flags.Info, "Byte order: {0}", BitConverter.IsLittleEndian ? "Little Endian" : "Big Endian");
//			Logger.Log(Flags.Info, "CLR {0}", Environment.Version);
		}
#endif


		/**
		 * Entry point
		 */
		public static void Main(string[] args)
		{
			string x = "foo";
			InspectorHelper.Inspect(ref x);
		}


		private static void __break()
		{
			Console.ReadLine();
		}

		#region todo

		private static void VMMap()
		{
			var table = new ConsoleTable("Low address", "High address", "Size");
			table.AddRow(Hex.ToHex(Memory.StackBase), Hex.ToHex(Memory.StackLimit),
				String.Format("{0} ({1} K)", Memory.StackSize, Memory.StackSize / 1024));
			Console.WriteLine(InspectorHelper.CreateLabelString("Stack:", table));

			table.Rows.RemoveAt(0);

			table.AddRow(Hex.ToHex(GCHeap.LowestAddress), Hex.ToHex(GCHeap.HighestAddress),
				String.Format("{0} ({1} K)", GCHeap.Size, GCHeap.Size / 1024));
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


		private struct UObj<T> : IDisposable
		{
			private IntPtr m_addr;

			public IntPtr Address => m_addr;

			internal UObj(IntPtr p)
			{
				m_addr = p;
			}

			/// <summary>
			///     Don't create a new instance using this
			/// </summary>
			public ref T Reference {
				get {
					IntPtr ptrPtr = AddressOf(ref m_addr);
					return ref Memory.AsRef<T>(ptrPtr);
				}
			}

			public void Dispose()
			{
				Marshal.FreeHGlobal(m_addr - IntPtr.Size);
			}

			public override string ToString()
			{
				return Reference.ToString();
			}
		}


		private static UObj<T> New<T>() where T : class
		{
			RazorContract.Assert(!typeof(T).IsArray);
			RazorContract.Assert(typeof(T) != typeof(string));
			RazorContract.Assert(!typeof(T).IsIListType());


			Pointer<byte> lpMem = Memory.AllocUnmanaged<byte>(BaseInstanceSize<T>());
			lpMem.Increment(sizeof(long));
			lpMem.Write((long) Runtime.MethodTableOf<T>());


			return new UObj<T>(lpMem.Address);
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