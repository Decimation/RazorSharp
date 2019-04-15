#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using InlineIL;
using RazorCommon;
using RazorCommon.Extensions;
using RazorCommon.Strings;
using RazorSharp;
using RazorSharp.CoreClr;
using RazorSharp.CoreClr.Structures;
using RazorSharp.CoreClr.Structures.ILMethods;
using RazorSharp.CoreJit;
using RazorSharp.Memory;
using RazorSharp.Memory.Calling.Symbols.Attributes;
using RazorSharp.Memory.Pointers;
using RazorSharp.Utilities;
using CSUnsafe = System.Runtime.CompilerServices.Unsafe;
using Unsafe = RazorSharp.Memory.Unsafe;

#endregion


namespace Test
{
	#region

	using DWORD = UInt32;
	using Ptr = Pointer<byte>;

	#endregion


	public static unsafe class Program
	{
		// todo: replace native pointers* with Pointer<T> for consistency
		// todo: RazorSharp, ClrMD, Reflection, Cecil, dnlib, MetadataTools comparison

		// Common library: RazorCommon
		// Testing library: RazorSandbox

		[ClrSymcall(Symbol = "Object::GetSize", FullyQualified = true)]
		private static int Size(this object obj)
		{
			return Constants.INVALID_VALUE;
		}

		// getILIntrinsicImplementation
		// https://github.com/dotnet/coreclr/blob/master/src/vm/jitinterface.cpp#L6961
		// https://github.com/dotnet/coreclr/blob/master/src/vm/jitinterface.cpp#L7090


		private static long ADDR = Clr.GetClrSymAddress("JIT_GetRuntimeType").ToInt64();

		public static Type Calli(long md)
		{
			IL.Emit.Ldarg_0();
			IL.Emit.Ldsfld(new FieldRef(typeof(Program), nameof(ADDR)));
			IL.Emit.Conv_I();
			IL.Emit.Calli(new StandAloneMethodSig(CallingConventions.Standard,
			                                      new TypeRef(typeof(Type)),
			                                      new TypeRef(typeof(void*))));
			return IL.Return<Type>();
		}

		

		private static IntPtr _corJitInfo;

		private static CompilerHook hook;

		private static byte[] _ilcode = new[]
		{
			(byte) OpCodes.Ldarg_0.Value,
			(byte) OpCodes.Conv_U.Value,
			(byte) OpCodes.Ret.Value
		};

		private static CorJitResult Compile(IntPtr     thisPtr,
		                                    IntPtr     corJitInfo,
		                                    CorInfo*   methInfo,
		                                    CorJitFlag flags,
		                                    IntPtr     nativeEntry,
		                                    IntPtr     nativeSizeOfCode)
		{
			var res = hook.Compile(thisPtr, corJitInfo, methInfo, flags, nativeEntry, nativeSizeOfCode);

			Console.WriteLine("JIT: {0:X}", thisPtr.ToInt64());
			Console.WriteLine("CorJITInfo: {0:X}", corJitInfo.ToInt64());
			Console.WriteLine("Method: {0:X}", methInfo->methodHandle.ToInt64());
			Console.WriteLine("Native entry: {0:X}", nativeEntry.ToInt64());
			Console.WriteLine("Native size: {0}", Marshal.ReadInt32(nativeSizeOfCode));
			Console.WriteLine("Tk {0:X}", (int) methInfo->args.token);
			Console.WriteLine("Result: {0}\n", res);

			// // Return the argument that was passed in.
			// static const BYTE ilcode[] = { CEE_LDARG_0, CEE_CONV_U, CEE_RET };
			// methInfo->ILCode = const_cast<BYTE*>(ilcode);
			// methInfo->ILCodeSize = sizeof(ilcode);
			// methInfo->maxStack = 1;
			// methInfo->EHcount = 0;
			// methInfo->options = (CorInfoOptions)0;


			_corJitInfo = corJitInfo;

			return res;
		}

		private static int Calc(int x, int y)
		{
			var r = Math.Asin(x);
			return (int) r * y;
		}


		private static void* AsPointer<T>(ref T t)
		{
			return null;
		}

		public static TTo As<TFrom, TTo>(TFrom source)
		{
			IL.Emit.Ldarg(nameof(source));
			return IL.Return<TTo>();
		}

		static int add(int a, int b) => a + b;

		
		public static void Main(string[] args)
		{
			// ICorJitCompiler
			var pJit = CorJitCompiler.GetJit();
			var m   = typeof(Program).GetAnyMethod(nameof(add));
			var il = m.GetMethodBody().GetILAsByteArray();
			Console.WriteLine(il.AutoJoin());
			Pointer<ILMethod> ilh = m.GetMethodDesc().Reference.GetILHeader();
			Console.WriteLine(ilh.Reference.GetILAsByteArray().AutoJoin());
			ILDump.DumpILToConsole(m);
		}
	}
}