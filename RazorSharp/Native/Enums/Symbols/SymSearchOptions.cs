using System;

namespace RazorSharp.Native.Enums.Symbols
{
	
	[Flags]
	internal enum SymSearchOptions
	{
		MaskObjs    = 0x01, // used internally to implement other APIs
		Recurse     = 0x02, // recurse scopes
		GlobalsOnly = 0x04, // search only for global symbols
		AllItems    = 0x08, // search for everything in the pdb, not just normal scoped symbols
	}
}