using System;

namespace RazorSharp.Memory.Extern.Symbols.Attributes
{
	[Flags]
	public enum ImportOptions
	{
		None = 0,
		
		/// <summary>
		/// Don't use <see cref="SymNamespaceAttribute.Namespace"/> in the symbol name resolution.
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
		/// Whether this should be interpreted as a global variable.
		/// <remarks>
		/// This is a combination of <see cref="IgnoreNamespace"/>, <see cref="IgnoreEnclosingNamespace"/>
		/// </remarks>
		/// </summary>
		Global = IgnoreNamespace | IgnoreEnclosingNamespace
	}
}