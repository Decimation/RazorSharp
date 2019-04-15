using System;
using System.Runtime.InteropServices;

namespace RazorSharp.CoreJit
{
	//CORINFO_SIG_INST
	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct CorInfoSigInst
	{
		internal UInt64  classInstCount;
		internal IntPtr* classInst; // (representative, not exact) instantiation for class type variables in signature
		internal UInt64  methInstCount;
		internal IntPtr* methInst; // (representative, not exact) instantiation for method type variables in signature
	}
}