using System;
using System.Runtime.InteropServices;

namespace RazorSharp.CoreJit
{
	//corinfo.h


	//CORINFO_RUNTIME_LOOKUP
	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct CorInfoRuntimeLookup
	{
		// This is signature you must pass back to the runtime lookup helper
		internal void* signature;

		// Here is the helper you must call. It is one of CORINFO_HELP_RUNTIMEHANDLE_* helpers.
		internal CorInfoHelpFunc helper;

		// Number of indirections to get there
		// CORINFO_USEHELPER = don't know how to get it, so use helper function at run-time instead
		// 0 = use the this pointer itself (e.g. token is C<!0> inside code in sealed class C)
		//     or method desc itself (e.g. token is method void M::mymeth<!!0>() inside code in M::mymeth)
		// Otherwise, follow each byte-offset stored in the "offsets[]" array (may be negative)
		internal UInt16 indirections;

		// If set, test for null and branch to helper if null
		internal bool testForNull;

		// If set, test the lowest bit and dereference if set (see code:FixupPointer)
		internal bool testForFixup;

		internal IntPtr offsets; //UInt32[#define CORINFO_MAXINDIRECTIONS 4]

		// If set, first offset is indirect.
		// 0 means that value stored at first offset (offsets[0]) from pointer is next pointer, to which the next offset
		// (offsets[1]) is added and so on.
		// 1 means that value stored at first offset (offsets[0]) from pointer is offset1, and the next pointer is
		// stored at pointer+offsets[0]+offset1.
		internal bool indirectFirstOffset;

		// If set, second offset is indirect.
		// 0 means that value stored at second offset (offsets[1]) from pointer is next pointer, to which the next offset
		// (offsets[2]) is added and so on.
		// 1 means that value stored at second offset (offsets[1]) from pointer is offset2, and the next pointer is
		// stored at pointer+offsets[1]+offset2.
		internal bool indirectSecondOffset;
	}
}