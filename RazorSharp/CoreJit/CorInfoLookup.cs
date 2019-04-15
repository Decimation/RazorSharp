#region

using System.Runtime.InteropServices;

#endregion

namespace RazorSharp.CoreJit
{
	// Result of calling embedGenericHandle
	//CORINFO_LOOKUP
	[StructLayout(LayoutKind.Explicit)]
	internal struct CorInfoLookup
	{
		[FieldOffset(0)]
		internal CorInfoLookupKind lookupKind;

		// If kind.needsRuntimeLookup then this indicates how to do the lookup
		[FieldOffset(sizeof(bool) + sizeof(uint) + sizeof(ushort) +
#if _TARGET_X64_
			4
#else
		             2
#endif
		)]
		internal CorInfoRuntimeLookup runtimeLookup;

		// If the handle is obtained at compile-time, then this handle is the "exact" handle (class, method, or field)
		// Otherwise, it's a representative...  If accessType is
		//     IAT_VALUE --> "handle" stores the real handle or "addr " stores the computed address
		//     IAT_PVALUE --> "addr" stores a pointer to a location which will hold the real handle
		//     IAT_PPVALUE --> "addr" stores a double indirection to a location which will hold the real handle
		[FieldOffset(sizeof(bool) + sizeof(uint) + sizeof(ushort) +
#if _TARGET_X64_
			4
#else
		             2
#endif
		)]
		internal CorInfoConstLookup constLookup;
	}
}