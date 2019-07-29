using System;
using RazorSharp.Import.Attributes;

namespace RazorSharp.Import.Enums
{
	/// <summary>
	/// Specifies how the identifier will be resolved.
	/// </summary>
	[Flags]
	public enum IdentifierOptions
	{
		None = 0,
		
		/// <summary>
		/// Don't use <see cref="ImportNamespaceAttribute.Namespace"/> in the symbol name resolution.
		/// </summary>
		IgnoreNamespace = 1,
		
		/// <summary>
		/// Don't use the enclosing type's name in the symbol name resolution.
		/// </summary>
		IgnoreEnclosingNamespace = 2,
		
		/// <summary>
		/// If the method is a <c>get</c> accessor, replace the <c>get_</c> in the name with <c>Get</c>
		/// </summary>
		UseAccessorName = 4,
		
		/// <summary>
		/// Use only the symbol name.
		/// <remarks>
		/// This is a combination of <see cref="IgnoreNamespace"/>, <see cref="IgnoreEnclosingNamespace"/>.
		/// This can also be used for global variables.
		/// </remarks>
		/// 
		/// </summary>
		FullyQualified = IgnoreNamespace | IgnoreEnclosingNamespace,
	}
}