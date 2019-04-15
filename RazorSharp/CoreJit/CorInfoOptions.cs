namespace RazorSharp.CoreJit
{
	internal enum CorInfoOptions : uint
	{
		CORINFO_OPT_INIT_LOCALS = 0x00000010, // zero initialize all variables

		CORINFO_GENERICS_CTXT_FROM_THIS =
			0x00000020, // is this shared generic code that access the generic context from the this pointer?  If so, then if the method has SEH then the 'this' pointer must always be reported and kept alive.

		CORINFO_GENERICS_CTXT_FROM_METHODDESC =
			0x00000040, // is this shared generic code that access the generic context from the ParamTypeArg(that is a MethodDesc)?  If so, then if the method has SEH then the 'ParamTypeArg' must always be reported and kept alive. Same as CORINFO_CALLCONV_PARAMTYPE

		CORINFO_GENERICS_CTXT_FROM_METHODTABLE =
			0x00000080, // is this shared generic code that access the generic context from the ParamTypeArg(that is a MethodTable)?  If so, then if the method has SEH then the 'ParamTypeArg' must always be reported and kept alive. Same as CORINFO_CALLCONV_PARAMTYPE

		CORINFO_GENERICS_CTXT_MASK = CORINFO_GENERICS_CTXT_FROM_THIS |
		                             CORINFO_GENERICS_CTXT_FROM_METHODDESC |
		                             CORINFO_GENERICS_CTXT_FROM_METHODTABLE,

		CORINFO_GENERICS_CTXT_KEEP_ALIVE =
			0x00000100 // Keep the generics context alive throughout the method even if there is no explicit use, and report its location to the CLR
	}
}