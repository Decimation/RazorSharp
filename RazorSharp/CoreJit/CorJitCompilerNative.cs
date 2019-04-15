#pragma warning disable 649

namespace RazorSharp.CoreJit
{
	internal struct CorJitCompilerNative
	{
		internal CorJitCompiler.CompileMethod             CompileMethod;
		internal CorJitCompiler.ProcessShutdownWork       ProcessShutdownWork;
		internal CorJitCompiler.IsCacheCleanupRequiredDel IsCacheCleanupRequired;
		internal CorJitCompiler.GetMethodAttribs          GetMethodAttribs;
	}
}