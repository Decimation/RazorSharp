using System;
using System.Runtime.InteropServices;

namespace RazorSharp.CoreJit
{
	//CORINFO_SIG_INFO
	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct CorInfoSigInfo
	{
		//CorInfoCallConv
		internal CorInfoCallConv callConv;

		//CORINFO_CLASS_HANDLE
		internal IntPtr retTypeClass; // if the return type is a value class, this is its handle (enums are normalized)

		internal IntPtr
			retTypeSigClass; // returns the value class as it is in the sig (enums are not converted to primitives)

		internal CorInfoType    retType;
		internal byte           flags; // used by IL stubs code
		internal UInt16         numArgs;
		internal CorInfoSigInst sigInst; // information about how type variables are being instantiated in generic code
		internal IntPtr         args;
		internal IntPtr         pSig;

		internal UInt64 cbSig;

		//scope CORINFO_MODULE_HANDLE
		internal IntPtr moduleHandle; // passed to getArgClass
		internal UInt32 token;
	}
}