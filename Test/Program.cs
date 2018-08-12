#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Running;
using ObjectLayoutInspector;
using RazorCommon;
using RazorCommon.Strings;
using RazorSharp;
using RazorSharp.Analysis;
using RazorSharp.Memory;
using RazorSharp.Pointers;
using RazorSharp.Utilities;
using Test.Testing;
using Test.Testing.Benchmarking;
using static RazorSharp.Unsafe;
using Runtime = RazorSharp.Runtime.Runtime;

#endregion

namespace Test
{

	#region

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
			Debug.Assert(IntPtr.Size == 8);
			Debug.Assert(Environment.Is64BitProcess);
			Logger.Log(Flags.Info, "Architecture: x64");
			Logger.Log(Flags.Info, "Byte order: {0}", BitConverter.IsLittleEndian ? "Little Endian" : "Big Endian");
			Logger.Log(Flags.Info, "CLR {0}", Environment.Version);
		}
#endif

		private static object InvokeGenericMethod(Type t, string name, Type typeArgs, object instance,
			params object[] args)
		{
			MethodInfo method = t.GetMethod(name,
				BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);
			method = method.MakeGenericMethod(typeArgs);
			return method.Invoke(method.IsStatic ? null : instance, args);
		}

		public static void Main(string[] args)
		{


			Point p = new Point();
			WriteOutVal(ref p);

			string s = "foo";
			WriteOut(ref s);


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


//			Console.ReadLine();
		}

		private static void WriteOutVal<T>(ref T t) where T : struct
		{
			Inspector<T>.Write(ref t, true);
		}

		private static void WriteOut<T>(ref T t) where T : class
		{
			RefInspector<T>.Write(ref t, true);
		}

		public struct Point
		{
			internal int _x;
			internal int _y;
		}


		private static void TableMethods<T>()
		{
			var table = new ConsoleTable("Function", "MethodDesc", "Name", "Virtual");
			foreach (var v in typeof(T).GetMethods(BindingFlags.Instance | BindingFlags.Public |
			                                       BindingFlags.NonPublic)) {
				table.AddRow(Hex.ToHex(v.MethodHandle.GetFunctionPointer()), Hex.ToHex(v.MethodHandle.Value),
					v.Name, v.IsVirtual ? StringUtils.Check : StringUtils.BallotX);
			}

			Console.WriteLine(table.ToMarkDownString());
		}

		private static void SetChar(this string str, int i, char c)
		{
			Pointer<char> lpChar = AddressOfHeap(ref str, OffsetType.StringData);
			lpChar[i] = c;
		}

		private static void RandomInit(AllocExPointer<string> ptr)
		{
			for (int i = 0; i < ptr.Count; i++) {
				ptr[i] = StringUtils.Random(10);
			}
		}

		/**
		 * Dependencies:
		 *
		 * RazorSharp:
		 *  - RazorCommon
		 * 	- CompilerServices.Unsafe
		 *  - RazorInvoke
		 *  - Fody
		 *  - MethodTimer Fody
		 *
		 * Test:
		 *  - RazorCommon
		 *  - CompilerServices.Unsafe
		 * 	- NUnit
		 *  - BenchmarkDotNet
		 *  - Fody
		 *  - MethodTimer Fody
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

	}

}