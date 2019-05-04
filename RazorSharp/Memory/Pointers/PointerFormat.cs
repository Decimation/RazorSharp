#region

using System;
using System.Collections.Generic;
using SimpleSharp;
using SimpleSharp.Strings;

#endregion

namespace RazorSharp.Memory.Pointers
{
	public static class PointerFormat
	{
		/// <summary>
		///     Object (<see cref="P:RazorSharp.Memory.Pointers.Pointer`1.Reference" />).
		///     <list type="bullet">
		///         <item>
		///             <description>
		///                 If the <see cref="Pointer{T}" />'s type is <see cref="char" />, it will be
		///                 returned as a C-string represented as a <see cref="string" />.
		///             </description>
		///         </item>
		///         <item>
		///             <description>
		///                 If the <see cref="Pointer{T}" />'s type is a reference type, its string representation will be
		///                 returned along with its heap pointer in <c>"P"</c> format.
		///             </description>
		///         </item>
		///         <item>
		///             <description>
		///                 If the <see cref="Pointer{T}" />'s type is an <see cref="IList{T}" /> type, its
		///                 contents will be returned along with its heap pointer in <c>"P"</c> format.
		///             </description>
		///         </item>
		///         <item>
		///             <description>
		///                 If the <see cref="Pointer{T}" />'s type is a number type, its value will be returned as well its
		///                 value in <see cref="Hex.ToHex(long, ToStringOptions)" /> format.
		///             </description>
		///         </item>
		///     </list>
		/// </summary>
		public const string FORMAT_OBJ = "O";

		/// <summary>
		///     <para>
		///         Pointer (<see cref="P:RazorSharp.Memory.Pointers.Pointer`1.Address" />) in
		///         <see cref="Hex.ToHex(IntPtr, ToStringOptions)" /> format
		///     </para>
		/// </summary>
		public const string FORMAT_PTR = "P";

		/// <summary>
		///     Both <see cref="FORMAT_PTR" /> and <see cref="FORMAT_OBJ" />
		/// </summary>
		public const string FORMAT_BOTH = "B";

		/// <summary>
		///     64-bit integer (<see cref="Pointer{T}.ToInt64" />)
		/// </summary>
		public const string FORMAT_INT = "N";

		internal const string VAL_FMT = "{0} ({1})";

		public static string DefaultFormat { get; set; } = FORMAT_PTR;
	}
}