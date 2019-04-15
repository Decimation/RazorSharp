using System;
using System.Runtime.InteropServices;

namespace RazorSharp.CoreJit
{
	//CORINFO_CONST_LOOKUP
	[StructLayout(LayoutKind.Explicit)]
	internal unsafe struct CorInfoConstLookup
	{
		// If the handle is obtained at compile-time, then this handle is the "exact" handle (class, method, or field)
		// Otherwise, it's a representative... 
		// If accessType is
		//     IAT_VALUE   --> "handle" stores the real handle or "addr " stores the computed address
		//     IAT_PVALUE  --> "addr" stores a pointer to a location which will hold the real handle
		//     IAT_PPVALUE --> "addr" stores a double indirection to a location which will hold the real handle
		[FieldOffset(0)]
		internal InfoAccessType accessType;

		[FieldOffset(sizeof(Int32))]
		internal IntPtr handle;

		[FieldOffset(sizeof(Int32))]
		internal void* addr;
	}
}