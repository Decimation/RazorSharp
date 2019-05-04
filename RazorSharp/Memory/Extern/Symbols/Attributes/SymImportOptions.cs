using System;

namespace RazorSharp.Memory.Extern.Symbols.Attributes
{
	[Flags]
	public enum SymImportOptions
	{
		None = 0,
		
		/// <summary>
		///     Whether <see cref="SymImportAttribute.Symbol" /> is the fully qualified name
		/// (don't use the member's name)
		/// </summary>
		FullyQualified = 1,
		
		/// <summary>
		///     Whether to use the decorated member's name as the symbol name.
		/// </summary>
		UseMemberNameOnly = 2,
		
		/// <summary>
		///     Whether to ignore <see cref="SymNamespaceAttribute.Namespace" /> if the enclosing type
		///     specifies a namespace.
		/// </summary>
		IgnoreNamespace = 4,
		
		/// <summary>
		/// If the method is a <c>get</c> accessor, replace the <c>get_</c> in the name with <c>Get</c>
		/// </summary>
		UseAccessorName = 8,
		
		
		/// <summary>
		/// Whether this should be interpreted as a global variable.
		/// <remarks>
		/// This is a combination of <see cref="IgnoreNamespace"/>, <see cref="UseMemberNameOnly"/>
		/// and <see cref="FullyQualified"/>
		/// </remarks>
		/// </summary>
		Global = IgnoreNamespace | FullyQualified | UseMemberNameOnly
	}
}