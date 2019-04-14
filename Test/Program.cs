#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
using RazorSharp.CorJit;
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

		// IntPtr      thisPtr,
		// [In] IntPtr corJitInfo,
		// [In] CorInfo* methodInfo,
		//               CorJitFlags.CorJitFlag flags,
		// [Out] IntPtr                         nativeEntry,
		// [Out] IntPtr                         nativeSizeOfCode

		private static IntPtr corJitInfo__;

		private static CompilerHook hook;

		private static CorJitCompiler.CorJitResult Compile(IntPtr                 thisPtr,
		                                                       IntPtr                 corJitInfo,
		                                                       CorInfo*               methInfo,
		                                                       CorJitFlag flags,
		                                                       IntPtr                 nativeEntry,
		                                                       IntPtr                 nativeSizeOfCode)
		{
			var res = hook.Compile(thisPtr, corJitInfo, methInfo, flags, nativeEntry, nativeSizeOfCode);

			Console.WriteLine("JIT: {0:X}", thisPtr.ToInt64());
			Console.WriteLine("CorJITInfo: {0:X}", corJitInfo.ToInt64());
			Console.WriteLine("Method: {0:X}", methInfo->methodHandle.ToInt64());
			Console.WriteLine("Native entry: {0:X}", nativeEntry.ToInt64());
			Console.WriteLine("Native size: {0}", Marshal.ReadInt32(nativeSizeOfCode));
			Console.WriteLine("Result: {0}\n", res);


			corJitInfo__ = corJitInfo;

			return res;
		}

		static int Calc(int x, int y)
		{
			var r = Math.Asin((double) x);
			return (int) r * y;
		}

		static string doS()
		{
			return "foo";
		}

		private static void CompileV(Type t, string name)
		{
			var fn = t.GetAnyMethod(name);
			RuntimeHelpers.PrepareMethod(fn.MethodHandle);
		}

		private static void CompileV<T>(string name)
		{
			var fn = typeof(T).GetAnyMethod(name);
			RuntimeHelpers.PrepareMethod(fn.MethodHandle);
		}

		public static TTo As<TFrom, TTo>(TFrom source)
		{
			IL.Emit.Ldarg(nameof(source));
			return IL.Return<TTo>();
		}

		public static void Main(string[] args)
		{
			// ICorJitCompiler
			var pJit = CorJitCompiler.GetJit();

			hook = new CompilerHook();

			

			var m = typeof(MethodBase).GetMethods()
			                          .Where(x => x.Name == "GetMethodFromHandle")
			                          .First(x => x.GetParameters().Length == 1 &&
			                                      x.GetParameters()[0].Name == "handle");

			RuntimeHelpers.PrepareMethod(m.MethodHandle);

			var tgt  = typeof(Program).GetAnyMethod("Calc");
			var tgt2 = typeof(Program).GetAnyMethod("doS");

			hook.Hook(Compile);

			RuntimeHelpers.PrepareMethod(tgt.MethodHandle);

			while (corJitInfo__ == IntPtr.Zero) { }

			RuntimeHelpers.PrepareMethod(tgt2.MethodHandle);

			hook.RemoveHook();


			
		}
	}
}