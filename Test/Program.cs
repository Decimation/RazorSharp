#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using RazorCommon;
using RazorCommon.Strings;
using RazorSharp;
using RazorSharp.Analysis;
using RazorSharp.CLR;
using RazorSharp.Memory;
using RazorSharp.Pointers;
using RazorSharp.Utilities;
using Test.Testing;
using static RazorSharp.Unsafe;

#endregion

namespace Test
{

	#region

	using CSUnsafe = Unsafe;

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
			Trace.Assert(IntPtr.Size == 8);
			Trace.Assert(Environment.Is64BitProcess);

//			Logger.Log(Flags.Info, "Architecture: x64");
//			Logger.Log(Flags.Info, "Byte order: {0}", BitConverter.IsLittleEndian ? "Little Endian" : "Big Endian");
//			Logger.Log(Flags.Info, "CLR {0}", Environment.Version);
		}
#endif


		private static T* AddrOf<T>(ref T t) where T : unmanaged
		{
			return (T*) AddressOf(ref t);
		}

		private static readonly Thread GCWatcherThread = new Thread(Watch);

		private static void Watch()
		{
			while (GCWatcherThread.IsAlive) {
				bool b = GCHeap.IsGCInProgress();

				Console.Write("\r{0} | {1}", b, GCHeap.GCCount);
			}
		}


		public static void Main(string[] args)
		{
			// todo: implement dynamic allocation system


/*			Klass k  = new Klass();
			var   md = GetMethodDesc<Klass>("increment");

			k.increment();
			Console.WriteLine("Orig:");
			Console.WriteLine(md->ToString());
			var orig = md->Function;

			Action a = delegate { Console.WriteLine("\nHook\n"); };
			md->SetFunctionPointer(a.Method.MethodHandle.GetFunctionPointer());
			Console.WriteLine("Hook:");
			Console.WriteLine(md->ToString());
			k.increment();

			// todo: fix
			md->SetFunctionPointer(orig);
			Console.WriteLine("Restore:");
			Console.WriteLine(md->ToString());
			md->Prepare();
			k.increment();*/


			GCWatcherThread.Start();

			for (int i = 0; i < 10; i++) {
				TestingUtil.CreatePressure();
			}

			GCWatcherThread.Abort();

			Console.WriteLine();

			Console.WriteLine(GCHeap.GCCount);


		}


		private static void DumpArray<T>(ref T[] arr) where T : class
		{
			Pointer<long> lp_rg = AddressOfHeap(ref arr, OffsetType.ArrayData);
			for (int i = 0; i < arr.Length; i++) {
				Console.WriteLine("{0} : {1}", Hex.ToHex(lp_rg.Read<long>()), lp_rg.Read<T>());
				lp_rg++;
			}
		}

		private static void TestTypes()
		{
			/**
			 * string
			 *
			 * Generic:		no
			 * Type:		reference
			 */
			string s = "foo";
			WriteOut(ref s);

			/**
			 * int[]
			 *
			 * Generic:		no
			 * Type:		reference
			 */
			int[] arr = {1, 2, 3};
			WriteOut(ref arr, true);

			/**
			 * string[]
			 *
			 * Generic:		no
			 * Type:		reference
			 */
			string[] ptrArr = {"foo", "bar", "desu"};
			WriteOut(ref ptrArr);

			/**
			 * int
			 *
			 * Generic:		no
			 * Type:		value
			 */
			int i = 0xFF;
			WriteOutVal(ref i);

			/**
			 * Dummy
			 *
			 * Generic:		no
			 * Type:		reference
			 */
			Dummy d = new Dummy();
			WriteOut(ref d);

			/**
			 * List<int>
			 *
			 * Generic:		yes
			 * Type:		reference
			 */
			List<int> ls = new List<int>();
			WriteOut(ref ls);

			/**
			 * Point
			 *
			 * Generic:		no
			 * Type:		value
			 *
			 */
			Point pt = new Point();
			WriteOutVal(ref pt);

			/**
			 * char
			 */
			char c = '\0';
			WriteOutVal(ref c);
		}

		private static void WriteOutVal<T>(ref T t, bool printStructures = false) where T : struct
		{
			Inspector<T>.Write(ref t, printStructures);
		}

		private static void WriteOut<T>(ref T t, bool printStructures = false) where T : class
		{
			RefInspector<T>.Write(ref t, printStructures);
		}


		private static void TableMethods<T>()
		{
			ConsoleTable table = new ConsoleTable("Function", "MethodDesc", "Name", "Virtual");
			foreach (MethodInfo v in typeof(T).GetMethods(BindingFlags.Instance | BindingFlags.Public |
			                                              BindingFlags.NonPublic))
				table.AddRow(Hex.ToHex(v.MethodHandle.GetFunctionPointer()), Hex.ToHex(v.MethodHandle.Value),
					v.Name, v.IsVirtual ? StringUtils.Check : StringUtils.BallotX);

			Console.WriteLine(table.ToMarkDownString());
		}


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