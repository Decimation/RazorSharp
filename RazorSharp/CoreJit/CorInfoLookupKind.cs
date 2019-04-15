using System;
using System.Runtime.InteropServices;

namespace RazorSharp.CoreJit
{
	//CORINFO_LOOKUP_KIND
	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct CorInfoLookupKind
	{
		internal bool                     needsRuntimeLookup;
		internal CorInfoRuntimeLookupKind runtimeLookupKind;

		// The 'runtimeLookupFlags' and 'runtimeLookupArgs' fields
		// are just for internal VM / ZAP communication, not to be used by the JIT.
		internal UInt16 runtimeLookupFlags;
		internal void*  runtimeLookupArgs;
	}
}