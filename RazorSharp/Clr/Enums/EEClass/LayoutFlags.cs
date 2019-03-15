using System;
using RazorSharp.Clr.Structures.EE;

namespace RazorSharp.Clr.Enums.EEClass {
	/// <summary>
	///     <para>Sources:</para>
	///     <list type="bullet">
	///         <item>
	///             <description>/src/vm/class.h: 396</description>
	///         </item>
	///     </list>
	///     <remarks>
	///         Use with <see cref="EEClassLayoutInfo.Flags" />
	///     </remarks>
	/// </summary>
	[Flags]
	public enum LayoutFlags : byte
	{
		/// <summary>
		///     TRUE if the GC layout of the class is bit-for-bit identical
		///     to its unmanaged counterpart (i.e. no internal reference fields,
		///     no ansi-unicode char conversions required, etc.) Used to
		///     optimize marshaling.
		/// </summary>
		Blittable = 0x01,

		/// <summary>
		///     Is this type also sequential in managed memory?
		/// </summary>
		ManagedSequential = 0x02,

		/// <summary>
		///     When a sequential/explicit type has no fields, it is conceptually
		///     zero-sized, but actually is 1 byte in length. This holds onto this
		///     fact and allows us to revert the 1 byte of padding when another
		///     explicit type inherits from this type.
		/// </summary>
		ZeroSized = 0x04,

		/// <summary>
		///     The size of the struct is explicitly specified in the meta-data.
		/// </summary>
		HasExplicitSize = 0x08,

		/// <summary>
		///     Whether a native struct is passed in registers.
		/// </summary>
		NativePassInRegisters = 0x10,

		R4HFA = 0x10,
		R8HFA = 0x20
	}
}