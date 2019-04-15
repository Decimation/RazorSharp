namespace RazorSharp.CoreJit
{
	internal enum CorInfoTokenKind
	{
		CORINFO_TOKENKIND_Class  = 0x01,
		CORINFO_TOKENKIND_Method = 0x02,
		CORINFO_TOKENKIND_Field  = 0x04,
		CORINFO_TOKENKIND_Mask   = 0x07,

		// token comes from CEE_LDTOKEN
		CORINFO_TOKENKIND_Ldtoken = 0x10 | CORINFO_TOKENKIND_Class | CORINFO_TOKENKIND_Method | CORINFO_TOKENKIND_Field,

		// token comes from CEE_CASTCLASS or CEE_ISINST
		CORINFO_TOKENKIND_Casting = 0x20 | CORINFO_TOKENKIND_Class,

		// token comes from CEE_NEWARR
		CORINFO_TOKENKIND_Newarr = 0x40 | CORINFO_TOKENKIND_Class,

		// token comes from CEE_BOX
		CORINFO_TOKENKIND_Box = 0x80 | CORINFO_TOKENKIND_Class,

		// token comes from CEE_CONSTRAINED
		CORINFO_TOKENKIND_Constrained = 0x100 | CORINFO_TOKENKIND_Class,

		// token comes from CEE_NEWOBJ
		CORINFO_TOKENKIND_NewObj = 0x200 | CORINFO_TOKENKIND_Method,

		// token comes from CEE_LDVIRTFTN
		CORINFO_TOKENKIND_Ldvirtftn = 0x400 | CORINFO_TOKENKIND_Method
	}
}