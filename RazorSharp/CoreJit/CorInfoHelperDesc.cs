using System;
using System.Runtime.InteropServices;

namespace RazorSharp.CoreJit {
	//CORINFO_HELPER_DESC
	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct CorInfoHelperDesc
	{
		internal CorInfoHelpFunc helperNum;
		internal UInt16          numArgs;

		[StructLayout(LayoutKind.Explicit)]
		internal struct args
		{
			[FieldOffset(0)]
			UInt32 fieldHandle;

			[FieldOffset(0)]
			UInt32 methodHandle;

			[FieldOffset(0)]
			UInt32 classHandle;

			[FieldOffset(0)]
			UInt32 moduleHandle;

			[FieldOffset(0)]
			UInt32 constant;
		};
	}
}