#region

using System;
using System.Runtime.InteropServices;

#endregion

namespace RazorSharp.CoreJit
{
	//CORINFO_CALL_INFO
	[StructLayout(LayoutKind.Sequential)]
	internal struct CorInfoCallInfo
	{
		internal IntPtr hMethod;     //target method handle
		internal uint   methodFlags; //flags for the target method

		internal uint classFlags; //flags for CORINFO_RESOLVED_TOKEN::hClass

		internal CorInfoSigInfo sig;

		//Verification information
		internal uint verMethodFlags; // flags for CORINFO_RESOLVED_TOKEN::hMethod

		internal CorInfoSigInfo verSig;
		//All of the regular method data is the same... hMethod might not be the same as CORINFO_RESOLVED_TOKEN::hMethod


		//If set to:
		//  - CORINFO_ACCESS_ALLOWED - The access is allowed.
		//  - CORINFO_ACCESS_ILLEGAL - This access cannot be allowed (i.e. it is public calling private).  The
		//      JIT may either insert the callsiteCalloutHelper into the code (as per a verification error) or
		//      call throwExceptionFromHelper on the callsiteCalloutHelper.  In this case callsiteCalloutHelper
		//      is guaranteed not to return.
		//  - CORINFO_ACCESS_RUNTIME_CHECK - The jit must insert the callsiteCalloutHelper at the call site.
		//      the helper may return
		internal CorInfoIsAccessAllowedResult accessAllowed;
		internal CorInfoHelperDesc            callsiteCalloutHelper;

		// See above section on constraintCalls to understand when these are set to unusual values.
		internal CorInfoThisTransform thisTransform;

		internal CorInfoCallKind kind;
		internal bool            nullInstanceCheck;

		// Context for inlining and hidden arg
		internal IntPtr contextHandle;

		internal bool
			exactContextNeedsRuntimeLookup; // Set if contextHandle is approx handle. Runtime lookup is required to get the exact handle.

		// If kind.CORINFO_VIRTUALCALL_STUB then stubLookup will be set.
		// If kind.CORINFO_CALL_CODE_POINTER then entryPointLookup will be set.
		[StructLayout(LayoutKind.Explicit)]
		internal struct Lookup
		{
			[FieldOffset(0)]
			private readonly CorInfoLookup stubLookup;

			[FieldOffset(0)]
			private readonly CorInfoLookup codePointerLookup;
		}

		internal CorInfoConstLookup instParamLookup; // Used by Ready-to-Run

		internal bool secureDelegateInvoke;
	}
}