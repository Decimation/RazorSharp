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
using RazorSharp.Memory.Calling.Symbols;
using RazorSharp.Memory.Calling.Symbols.Attributes;
using RazorSharp.Memory.Pointers;
using RazorSharp.Native;
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

		// https://github.com/dotnet/coreclr/blob/9f1dc4444478ccbac2476d53949c471583876ad7/src/inc/corjit.h#L318

		// this function is for debugging only. It returns the method name
		// and if 'moduleName' is non-null, it sets it to something that will
		// says which method (a class name, or a module name)
//		virtual const char* __stdcall getMethodName (
//			CORINFO_METHOD_HANDLE ftn,          /* IN */
//		const char                ** moduleName /* OUT */
//			) = 0;

		[SymNamespace("CEEInfo")]
		struct ICorJitInfo
		{
			static ICorJitInfo()
			{
				
			}

			[ClrSymcall(UseMethodNameOnly = true)]
			public Pointer<byte> getMethodName(Pointer<byte> ftn, Pointer<Pointer<byte>> moduleName)
			{
				return null;
			}
		}

		private static void Dump(ICorJitInfo* info, CorInfo* corInfo)
		{
			byte* szMethodName = stackalloc byte[256];
			byte* szClassName  = stackalloc byte[256];

			var m  = info->getMethodName(corInfo->methodHandle, &szClassName);
			Console.WriteLine((char)m[0]);
			Console.ReadLine();

		}

		private static CompilerHook _compilerHook;

		private static CorJitResult CompileHook(IntPtr        thisPtr,
		                                        [In] IntPtr   corJitInfo,
		                                        [In] CorInfo* methodInfo,
		                                        CorJitFlag    flags,
		                                        [Out] IntPtr  nativeEntry,
		                                        [Out] IntPtr  nativeSizeOfCode)
		{
			var value = _compilerHook.Compile(thisPtr, corJitInfo, methodInfo, flags, nativeEntry, nativeSizeOfCode);
			Console.WriteLine(value);
			return value;
		}

		static void __Compile(Type t, string n)
		{
			RuntimeHelpers.PrepareMethod(t.GetAnyMethod(n).MethodHandle);
		}

		public static void Main(string[] args)
		{
			Symcall.BindQuick(typeof(ICorJitInfo));
			
			// ICorJitCompiler
			var pJit = CorJitCompiler.GetJit();

			__Compile(typeof(Program),"Dump");
			
			
			
			_compilerHook = new CompilerHook();
			_compilerHook.Hook(CompileHook);
		}
	}
}