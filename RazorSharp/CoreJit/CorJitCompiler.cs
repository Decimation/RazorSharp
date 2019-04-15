using System;
using System.Runtime.InteropServices;

namespace RazorSharp.CoreJit
{
	internal unsafe class CorJitCompiler
	{
		internal static ICorJitCompiler GetCorJitCompilerInterface()
		{
			var pJit           = GetJit();
			var nativeCompiler = Marshal.PtrToStructure<CorJitCompilerNative>(pJit);
			return new CorJitCompilerNativeWrapper(pJit, nativeCompiler.CompileMethod,
			                                       nativeCompiler.ProcessShutdownWork,
			                                       nativeCompiler.GetMethodAttribs);
		}

		private sealed class CorJitCompilerNativeWrapper : ICorJitCompiler
		{
			private          IntPtr                 _pThis;
			private readonly CompileMethodDel       _compileMethod;
			private readonly ProcessShutdownWorkDel _processShutdownWork;
			private readonly GetMethodAttribs       _getMethodAttribs;

			internal CorJitCompilerNativeWrapper(IntPtr                 pThis,
			                                   CompileMethodDel       compileMethodDel,
			                                   ProcessShutdownWorkDel processShutdownWork,
			                                   GetMethodAttribs       getMethodAttribs)
			{
				_pThis               = pThis;
				_compileMethod       = compileMethodDel;
				_processShutdownWork = processShutdownWork;
				_getMethodAttribs    = getMethodAttribs;
			}

			public CorJitResult CompileMethod(IntPtr        thisPtr,
			                                  [In] IntPtr   corJitInfo,
			                                  [In] CorInfo* methodInfo,
			                                  CorJitFlag    flags,
			                                  [Out] IntPtr  nativeEntry,
			                                  [Out] IntPtr  nativeSizeOfCode)
			{
				return _compileMethod(thisPtr, corJitInfo, methodInfo, flags, nativeEntry, nativeSizeOfCode);
			}

			public void ProcessShutdownWork(IntPtr thisPtr, [In] IntPtr corStaticInfo)
			{
				_processShutdownWork(thisPtr, corStaticInfo);
			}

			internal uint GetMethodAttribs(IntPtr methodHandle)
			{
				return _getMethodAttribs(methodHandle);
			}
		}

		// _TARGET_X64_ "Clrjit.dll"
		// else			"Mscorjit.dll"
		[DllImport("Clrjit.dll", CallingConvention = CallingConvention.StdCall,
			SetLastError                           = true,
			EntryPoint                             = "getJit",
			BestFitMapping                         = true)]
		internal static extern IntPtr GetJit();


		[UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
		internal delegate CorJitResult CompileMethodDel(IntPtr        thisPtr,
		                                              [In] IntPtr   corJitInfo,
		                                              [In] CorInfo* methodInfo,
		                                              CorJitFlag    flags,
		                                              [Out] IntPtr  nativeEntry,
		                                              [Out] IntPtr  nativeSizeOfCode);

		[UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
		internal delegate void ProcessShutdownWorkDel(IntPtr thisPtr, [Out] IntPtr corStaticInfo);

		[UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
		internal delegate byte IsCacheCleanupRequiredDel();

		[UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
		internal delegate uint GetMethodAttribs(IntPtr methodHandle);


		internal enum CodeOptimize
		{
			BLENDED_CODE,
			SMALL_CODE,
			FAST_CODE,
			COUNT_OPT_CODE
		}
	}
}