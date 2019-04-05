namespace RazorSharp.Native.Structures
{
	public enum SymType
	{
		SymNone = 0,
		SymCoff,
		SymCv,
		SymPdb,
		SymExport,
		SymDeferred,
		SymSym, // .sym file
		SymDia,
		SymVirtual,
		NumSymTypes
	}
}