#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using InlineIL;
using RazorCommon;
using RazorCommon.Extensions;
using RazorCommon.Strings;
using RazorSharp;
using RazorSharp.CoreClr;
using RazorSharp.CoreClr.Structures;
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
		
		private static Jit.CorJitCompiler.CorJitResult Compile(IntPtr                 thisPtr, IntPtr corJitInfo,
		                                              CorInfo*               methInfo,
		                                              CorJitFlags.CorJitFlag flags, IntPtr nativeEntry,
		                                              IntPtr                 nativeSizeOfCode)
		{
			Console.WriteLine("JIT: {0:X}", thisPtr.ToInt64());
			Console.WriteLine("CorJITInfo: {0:X}", corJitInfo.ToInt64());
			corJitInfo__ = corJitInfo;
			return default;
		}
		
		static int Calc(int x, int y)
		{
			var r = Math.Asin((double)x);
			return (int)r * y;
		}
		
		public static void Main(string[] args)
		{
			
			var hook = new CompilerHook();
			hook.Hook(Compile);

			SpinWait.SpinUntil(() => corJitInfo__ != IntPtr.Zero);
			hook.RemoveHook();
			RuntimeHelpers.PrepareMethod(typeof(Program).GetAnyMethod("Calc").MethodHandle);

			

		}
	}
}