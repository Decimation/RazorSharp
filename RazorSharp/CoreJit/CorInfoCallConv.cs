namespace RazorSharp.CoreJit
{
	internal enum CorInfoCallConv
	{
		// These correspond to CorCallingConvention

		CORINFO_CALLCONV_DEFAULT      = 0x0,
		CORINFO_CALLCONV_C            = 0x1,
		CORINFO_CALLCONV_STDCALL      = 0x2,
		CORINFO_CALLCONV_THISCALL     = 0x3,
		CORINFO_CALLCONV_FASTCALL     = 0x4,
		CORINFO_CALLCONV_VARARG       = 0x5,
		CORINFO_CALLCONV_FIELD        = 0x6,
		CORINFO_CALLCONV_LOCAL_SIG    = 0x7,
		CORINFO_CALLCONV_PROPERTY     = 0x8,
		CORINFO_CALLCONV_NATIVEVARARG = 0xb, // used ONLY for IL stub PInvoke vararg calls

		CORINFO_CALLCONV_MASK         = 0x0f, // Calling convention is bottom 4 bits
		CORINFO_CALLCONV_GENERIC      = 0x10,
		CORINFO_CALLCONV_HASTHIS      = 0x20,
		CORINFO_CALLCONV_EXPLICITTHIS = 0x40,
		CORINFO_CALLCONV_PARAMTYPE    = 0x80 // Passed last. Same as CORINFO_GENERICS_CTXT_FROM_PARAMTYPEARG
	}
}