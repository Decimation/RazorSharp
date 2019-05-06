#region

using System;

#endregion

namespace RazorSharp.CoreClr.Structures.Enums
{
	/// <summary>
	///     The value of lowest two bits describe what the union contains
	///     <remarks>
	///         Use with <see cref="Structures.MethodTable.UnionType" />
	///     </remarks>
	/// </summary>
	[Flags]
	internal enum LowBits
	{
		/// <summary>
		///     0 - pointer to <see cref="EEClass" />
		///     This <see cref="MethodTable" /> is the canonical method table.
		/// </summary>
		EEClass = 0,

		/// <summary>
		///     1 - not used
		/// </summary>
		Invalid = 1,

		/// <summary>
		///     2 - pointer to canonical <see cref="MethodTable" />.
		/// </summary>
		MethodTable = 2,

		/// <summary>
		///     3 - pointer to indirection cell that points to canonical <see cref="MethodTable" />.
		///     (used only if FEATURE_PREJIT is defined)
		/// </summary>
		Indirection = 3
	}
}