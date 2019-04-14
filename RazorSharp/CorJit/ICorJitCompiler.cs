using System;
using System.Runtime.InteropServices;

namespace RazorSharp.CorJit
{
	/// <summary>
	/// corjit.h
	/// </summary>
	/// 
	public unsafe interface ICorJitCompiler
	{
		CorJitResult CompileMethod(IntPtr                 thisPtr, [In] IntPtr corJitInfo,
		                                          [In] CorInfo*          methodInfo,
		                                          CorJitFlag flags, [Out] IntPtr nativeEntry,
		                                          [Out] IntPtr           nativeSizeOfCode);

		void ProcessShutdownWork(IntPtr thisPtr, [In] IntPtr corStaticInfo);
	}
}