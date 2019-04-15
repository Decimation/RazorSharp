// ReSharper disable UnassignedField.Global

#pragma warning disable 649

namespace RazorSharp.CoreJit
{
	internal struct CorJitCompilerNative
	{
		internal CorJitCompiler.CompileMethodDel          CompileMethod;
		internal CorJitCompiler.ProcessShutdownWorkDel    ProcessShutdownWork;
		internal CorJitCompiler.IsCacheCleanupRequiredDel IsCacheCleanupRequired;
		internal CorJitCompiler.GetMethodAttribs          GetMethodAttribs;
	}
}