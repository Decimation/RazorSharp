#region

using System;

#endregion

namespace RazorSharp.Native.Symbols
{
	[Flags]
	internal enum SymEnumOptions
	{
		Default = 0x01,
		Inline  = 0x02, // includes inline symbols

		All = Default | Inline
	}
}