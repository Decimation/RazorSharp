using System;

namespace RazorSharp.Native.Symbols
{
	[Flags]
	public enum SymbolFlag
	{
		/// <summary>
		/// The symbol is a CLR token.
		/// </summary>
		SYMFLAG_CLR_TOKEN = 0x00040000,


		/// <summary>
		/// The symbol is a constant.
		/// </summary>
		SYMFLAG_CONSTANT = 0x00000100,


		/// <summary>
		/// The symbol is from the export table.
		/// </summary>
		SYMFLAG_EXPORT = 0x00000200,


		/// <summary>
		/// The symbol is a forwarder.
		/// </summary>
		SYMFLAG_FORWARDER = 0x00000400,


		/// <summary>
		/// Offsets are frame relative.
		/// </summary>
		SYMFLAG_FRAMEREL = 0x00000020,


		/// <summary>
		/// The symbol is a known function.
		/// </summary>
		SYMFLAG_FUNCTION = 0x00000800,


		/// <summary>
		/// The symbol address is an offset relative to the beginning of the intermediate language block. This applies to managed code only.
		/// </summary>
		SYMFLAG_ILREL = 0x00010000,


		/// <summary>
		/// The symbol is a local variable.
		/// </summary>
		SYMFLAG_LOCAL = 0x00000080,


		/// <summary>
		/// The symbol is managed metadata.
		/// </summary>
		SYMFLAG_METADATA = 0x00020000,


		/// <summary>
		/// The symbol is a parameter.
		/// </summary>
		SYMFLAG_PARAMETER = 0x00000040,


		/// <summary>
		/// The symbol is a register. The Register member is used.
		/// </summary>
		SYMFLAG_REGISTER = 0x00000008,

		/// <summary>
		/// Offsets are register relative.
		/// </summary>
		SYMFLAG_REGREL = 0x00000010,

		/// <summary>
		/// The symbol is a managed code slot.
		/// </summary>
		SYMFLAG_SLOT = 0x00008000,

		/// <summary>
		/// The symbol is a thunk.
		/// </summary>
		SYMFLAG_THUNK = 0x00002000,

		/// <summary>
		/// The symbol is an offset into the TLS data area.
		/// </summary>
		SYMFLAG_TLSREL = 0x00004000,

		/// <summary>
		/// The Value member is used.
		/// </summary>
		SYMFLAG_VALUEPRESENT = 0x00000001,

		/// <summary>
		/// The symbol is a virtual symbol created by the SymAddSymbol function.
		/// </summary>
		SYMFLAG_VIRTUAL = 0x00001000,
	}
}