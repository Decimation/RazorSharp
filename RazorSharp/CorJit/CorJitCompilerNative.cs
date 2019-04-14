// ReSharper disable UnassignedField.Global

using static RazorSharp.CorJit.CorJitCompiler;

namespace RazorSharp.CorJit
{
	public struct CorJitCompilerNative
	{
		public CompileMethodDel          CompileMethod;
		public ProcessShutdownWorkDel    ProcessShutdownWork;
		public IsCacheCleanupRequiredDel IsCacheCleanupRequired;
		public GetMethodAttribs          GetMethodAttribs;
	}
}