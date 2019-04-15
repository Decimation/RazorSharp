using System;
using System.Runtime.InteropServices;

namespace RazorSharp.CoreJit
{
	[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x88)]
	internal struct CorInfo
	{
		//ftn CORINFO_METHOD_HANDLE
		internal IntPtr methodHandle;

		//scope CORINFO_MODULE_HANDLE
		internal IntPtr moduleHandle;

		//BYTE*
		internal IntPtr ILCode;
		internal UInt32 ILCodeSize;
		internal UInt16 maxStack;

		internal UInt16 EHcount;

		//options CorInfoOptions
		internal CorInfoOptions options;

		//regionKind CorInfoRegionKind
		internal CorInfoRegionKind regionKind;

		//CORINFO_SIG_INFO
		internal CorInfoSigInfo args;

		//CORINFO_SIG_INFO
		internal CorInfoSigInfo locals;
	}
}