#region

using System;
using System.Runtime.InteropServices;

#endregion

namespace RazorSharp.CoreJit
{
	//CORINFO_SIG_INST
	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct CorInfoSigInst
	{
		internal ulong   classInstCount;
		internal IntPtr* classInst; // (representative, not exact) instantiation for class type variables in signature
		internal ulong   methInstCount;
		internal IntPtr* methInst; // (representative, not exact) instantiation for method type variables in signature
	}
}