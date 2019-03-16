using System;
using RazorSharp.CoreClr.Structures;

namespace RazorSharp.CoreClr.Enums.MethodDesc
{
	/// <summary>
	///     <remarks>
	///         Use with <see cref="MethodDesc.Flags" />
	///     </remarks>
	/// </summary>
	[Flags]
	public enum MethodDescClassification : ushort
	{
		/// <summary>
		///     Method is <see cref="MethodClassification.IL" />, <see cref="MethodClassification.FCall" /> etc., see
		///     <see cref="MethodClassification" /> above.
		/// </summary>
		Classification = 0x0007,
		ClassificationCount = Classification + 1,

		// Note that layout of code:MethodDesc::s_ClassificationSizeTable depends on the exact values
		// of mdcHasNonVtableSlot and mdcMethodImpl

		/// <summary>
		///     Has local slot (vs. has real slot in MethodTable)
		/// </summary>
		HasNonVtableSlot = 0x0008,

		/// <summary>
		///     Method is a body for a method impl (MI_MethodDesc, MI_NDirectMethodDesc, etc)
		///     where the function explicitly implements IInterface.foo() instead of foo().
		/// </summary>
		MethodImpl = 0x0010,

		/// <summary>
		///     Method is static
		/// </summary>
		Static = 0x0020,

		// unused                           = 0x0040,
		// unused                           = 0x0080,
		// unused                           = 0x0100,
		// unused                           = 0x0200,

		// Duplicate method. When a method needs to be placed in multiple slots in the
		// method table, because it could not be packed into one slot. For eg, a method
		// providing implementation for two interfaces, MethodImpl, etc
		Duplicate = 0x0400,

		/// <summary>
		///     Has this method been verified?
		/// </summary>
		VerifiedState = 0x0800,

		/// <summary>
		///     Is the method verifiable? It needs to be verified first to determine this
		/// </summary>
		Verifiable = 0x1000,

		/// <summary>
		///     Is this method ineligible for inlining?
		/// </summary>
		NotInline = 0x2000,

		/// <summary>
		///     Is the method synchronized
		/// </summary>
		Synchronized = 0x4000,

		/// <summary>
		///     Does the method's slot number require all 16 bits
		/// </summary>
		RequiresFullSlotNumber = 0x8000
	}
}