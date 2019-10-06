#region

using System;
using System.Collections.Generic;
using RazorSharp.Interop;
using RazorSharp.Memory.Pointers;

#endregion

namespace RazorSharp.Import.Enums
{
	/// <summary>
	/// Designates how the annotated function is imported.
	/// </summary>
	[Flags]
	public enum ImportCallOptions
	{
		None = 0,

		/// <summary>
		///     Treat the method as a constructor. <see cref="IdentifierOptions.FullyQualified" /> must be used.
		/// </summary>
		Constructor = 1,

		/// <summary>
		///     Sets the method entry point to the resolved address. This only works on 64 bit.
		/// </summary>
		Bind = 1 << 1,

		/// <summary>
		///     <para>
		///         Adds the resolved address to an <see cref="ImportMap"/> in the enclosing type
		///     </para>
		///     <para>
		///         The key of the import map is the name of the annotated member. The name is obtained using the
		///         <c>nameof</c> operator. The value is the resolved address.
		///     </para>
		///     <para>Best used in conjunction with the functions in <see cref="Functions.Native" /></para>
		/// </summary>
		Map = 1 << 2
	}
}