#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using RazorCommon;
using RazorCommon.Strings;
using RazorSharp.Analysis;
using RazorSharp.Pointers;
using RazorSharp.Utilities;
using Test.Testing;
using static RazorSharp.Unsafe;

#endregion

namespace Test
{

	#region

	using CSUnsafe = System.Runtime.CompilerServices;

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
			Debug.Assert(IntPtr.Size == 8);
			Debug.Assert(Environment.Is64BitProcess);

//			Logger.Log(Flags.Info, "Architecture: x64");
//			Logger.Log(Flags.Info, "Byte order: {0}", BitConverter.IsLittleEndian ? "Little Endian" : "Big Endian");
//			Logger.Log(Flags.Info, "CLR {0}", Environment.Version);
		}
#endif


		private static T* AddrOf<T>(ref T t) where T : unmanaged
		{
			return (T*) AddressOf(ref t);
		}


		struct SStruct
		{
			public static string Static;
			public        string Instance;

		}

		class CClass
		{
			public        string Instance;
			public static string Static;
		}

		public static void Main(string[] args)
		{
			SStruct ss;
			ss.Instance    = "bar";
			SStruct.Static = "foo";

			WriteOutVal(ref ss, true);

			CClass cc = new CClass();
			WriteOut(ref cc, true);

			Console.ReadLine();
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
		 * Reflection							CLR
		 *
		 * FieldInfo.MetadataToken				FieldDesc.MemberDef
		 * FieldInfo::FieldHandle.Value			FieldDesc*
		 * CorElementType						FieldDesc.Type
		 * MethodInfo::MethodHandle.Value		MethodDesc*
		 * Type::TypeHandle.Value				MethodTable*
		 * Type::Attributes						EEClass.m_dwAttrClass
		 * Marshal::SizeOf						EEClass.m_cbNativeSize
		 *
		 */

		/**
		 * #defines:
		 *
		 * FEATURE_COMINTEROP
		 * _TARGET_64BIT_
		 */

	}

}