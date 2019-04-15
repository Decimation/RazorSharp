#region

using System;
using System.Runtime.InteropServices;

// ReSharper disable ClassNeverInstantiated.Global

#endregion

namespace RazorSharp.CoreJit
{
	internal unsafe class CorJitCompiler
	{
		internal static ICorJitCompiler GetCorJitCompilerInterface() => GetCorJitCompilerInterface(GetJit());

		internal static ICorJitCompiler GetCorJitCompilerInterface(IntPtr pJit)
		{
			var nativeCompiler = Marshal.PtrToStructure<CorJitCompilerNative>(pJit);
			return new CorJitCompilerNativeWrapper(pJit, nativeCompiler.CompileMethod,
			                                       nativeCompiler.ProcessShutdownWork,
			                                       nativeCompiler.GetMethodAttribs);
		}

		// _TARGET_X64_ "Clrjit.dll"
		// else			"Mscorjit.dll"

		/// <summary>
		/// ICorJitCompiler
		/// </summary>
		[DllImport("clrjit.dll", CallingConvention = CallingConvention.StdCall,
			SetLastError                           = true,
			EntryPoint                             = "getJit",
			BestFitMapping                         = true)]
		internal static extern IntPtr GetJit();

		private sealed class CorJitCompilerNativeWrapper : ICorJitCompiler
		{
			private readonly CompileMethod       _compileMethod;
			private readonly GetMethodAttribs    _getMethodAttribs;
			private readonly ProcessShutdownWork _processShutdownWork;
			private          IntPtr              _pThis;

			internal CorJitCompilerNativeWrapper(IntPtr              pThis,
			                                     CompileMethod       compileMethod,
			                                     ProcessShutdownWork processShutdownWork,
			                                     GetMethodAttribs    getMethodAttribs)
			{
				_pThis               = pThis;
				_compileMethod       = compileMethod;
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

			public uint GetMethodAttribs(IntPtr methodHandle)
			{
				return _getMethodAttribs(methodHandle);
			}
		}


		[UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
		internal delegate CorJitResult CompileMethod(IntPtr        thisPtr,
		                                             [In] IntPtr   corJitInfo,
		                                             [In] CorInfo* methodInfo,
		                                             CorJitFlag    flags,
		                                             [Out] IntPtr  nativeEntry,
		                                             [Out] IntPtr  nativeSizeOfCode);

		[UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
		internal delegate void ProcessShutdownWork(IntPtr thisPtr, [Out] IntPtr corStaticInfo);

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