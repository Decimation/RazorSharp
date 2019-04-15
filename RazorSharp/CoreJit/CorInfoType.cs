namespace RazorSharp.CoreJit
{
	// The enumeration is returned in 'getSig','getType', getArgType methods
	internal enum CorInfoType
	{
		CORINFO_TYPE_UNDEF      = 0x0,
		CORINFO_TYPE_VOID       = 0x1,
		CORINFO_TYPE_BOOL       = 0x2,
		CORINFO_TYPE_CHAR       = 0x3,
		CORINFO_TYPE_BYTE       = 0x4,
		CORINFO_TYPE_UBYTE      = 0x5,
		CORINFO_TYPE_SHORT      = 0x6,
		CORINFO_TYPE_USHORT     = 0x7,
		CORINFO_TYPE_INT        = 0x8,
		CORINFO_TYPE_UINT       = 0x9,
		CORINFO_TYPE_LONG       = 0xa,
		CORINFO_TYPE_ULONG      = 0xb,
		CORINFO_TYPE_NATIVEINT  = 0xc,
		CORINFO_TYPE_NATIVEUINT = 0xd,
		CORINFO_TYPE_FLOAT      = 0xe,
		CORINFO_TYPE_DOUBLE     = 0xf,
		CORINFO_TYPE_STRING     = 0x10, // Not used, should remove
		CORINFO_TYPE_PTR        = 0x11,
		CORINFO_TYPE_BYREF      = 0x12,
		CORINFO_TYPE_VALUECLASS = 0x13,
		CORINFO_TYPE_CLASS      = 0x14,
		CORINFO_TYPE_REFANY     = 0x15,

		// CORINFO_TYPE_VAR is for a generic type variable.
		// Generic type variables only appear when the JIT is doing
		// verification (not NOT compilation) of generic code
		// for the EE, in which case we're running
		// the JIT in "import only" mode.

		CORINFO_TYPE_VAR = 0x16,
		CORINFO_TYPE_COUNT, // number of jit types
	};
}