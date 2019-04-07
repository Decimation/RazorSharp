#region

using System;
using RazorSharp.CoreClr.Structures.ILMethods;

#endregion

// ReSharper disable InconsistentNaming

namespace RazorSharp.CoreClr.Structures
{
	/// <summary>
	///     Use with <see cref="FatILMethod.FlagsValue" /> or <see cref="TinyILMethod.Flags_CodeSize" />.
	///     The only semantic flag at present is <see cref="InitLocals" />
	/// </summary>
	[Flags]
	public enum CorILMethodFlags : byte
	{
		/// <summary>
		///     Call default constructor on all local vars
		/// </summary>
		InitLocals = 0x0010,

		/// <summary>
		///     There is another attribute after this one
		/// </summary>
		MoreSects = 0x0008,

		/// <summary>
		///     Not used.
		/// </summary>
		CompressedIL = 0x0040,

		/// <summary>
		///     Indicates the format for the <see cref="ILMethod" /> header
		/// </summary>
		FormatShift = 3,
		FormatMask = (1 << FormatShift) - 1,

		/// <summary>
		///     Use this code if the code size is even
		/// </summary>
		TinyFormat = 0x0002,
		SmallFormat = 0x0000,
		FatFormat   = 0x0003,

		/// <summary>
		///     use this code if the code size is odd
		/// </summary>
		TinyFormat1 = 0x0006
	}
}