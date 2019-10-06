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
		Constructor = 1 << 0,

		/// <summary>
		///     Sets the method entry point to the resolved address. This only works on 64 bit.
		/// </summary>
		Bind = 1 << 1,

		/// <summary>
		///     <para>
		///         Adds the resolved address to an import map (a <see cref="Dictionary{TKey,TValue}" /> in the enclosing type)
		///         with the key being a <see cref="string" /> and the value being
		///         a <see cref="byte" /> <see cref="Pointer{T}" />.
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