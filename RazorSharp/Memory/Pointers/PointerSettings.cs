using System;
using System.Collections.Generic;
using SimpleSharp.Strings.Formatting;

namespace RazorSharp.Memory.Pointers
{
	internal static class PointerSettings
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
		///                 returned along with its heap pointer in <see cref="FORMAT_PTR"/> format.
		///             </description>
		///         </item>
		///         <item>
		///             <description>
		///                 If the <see cref="Pointer{T}" />'s type is an <see cref="IList{T}" /> type, its
		///                 contents will be returned along with its heap pointer in <see cref="FORMAT_PTR"/> format.
		///             </description>
		///         </item>
		///         <item>
		///             <description>
		///                 If the <see cref="Pointer{T}" />'s type is a number type, its value will be returned as well its
		///                 value in <see cref="Hex.ToHex(long, HexOptions)" /> format.
		///             </description>
		///         </item>
		///     </list>
		/// </summary>
		internal const string FORMAT_OBJ = "O";

		/// <summary>
		///     <para>
		///         Pointer (<see cref="P:RazorSharp.Memory.Pointers.Pointer`1.Address" />) in
		///         <see cref="Hex.ToHex(IntPtr, HexOptions)" /> format
		///     </para>
		/// </summary>
		internal const string FORMAT_PTR = "P";

		/// <summary>
		///     Both <see cref="FORMAT_PTR" /> and <see cref="FORMAT_OBJ" />
		/// </summary>
		internal const string FORMAT_BOTH = "B";

		/// <summary>
		///     64-bit integer
		/// </summary>
		internal const string FORMAT_INT = "N";

		/// <summary>
		/// Displays <see cref="ArrayCount"/> elements
		/// </summary>
		internal const string FORMAT_ARRAY = "RG";

		internal const string VAL_FMT = "{0} ({1})";

		internal const string CHAR_PTR = "Char*";

		/// <summary>
		/// Number of elements to display when using <see cref="FORMAT_ARRAY"/>
		/// </summary>
		internal static int ArrayCount { get; set; } = 6;
		
		/// <summary>
		/// Default format specifier
		/// </summary>
		internal static string DefaultFormat { get; set; } = FORMAT_PTR;
	}
}