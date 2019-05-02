using System;
using System.Diagnostics;

namespace RazorSharp.Memory.Extern.Symbols.Attributes
{
	/// <summary>
	/// Indicates a resource is imported from a symbol file and retrieved from its corresponding <see cref="ProcessModule"/>
	/// by adding the symbol's RVA to the module's <see cref="ProcessModule.BaseAddress"/>
	/// <seealso cref="ModuleInfo"/>
	/// </summary>
	public class SymImportAttribute : Attribute
	{
		// todo: this configuration is a bit confusing

		/// <summary>
		///     Name of the symbol
		/// </summary>
		public string Symbol { get; set; }

		/// <summary>
		///     Whether <see cref="Symbol" /> is the fully qualified name (don't use the member's name)
		/// </summary>
		public bool FullyQualified { get; set; }

		/// <summary>
		///     Whether to use the decorated member's name as the symbol.
		/// </summary>
		public bool UseMemberNameOnly { get; set; }

		/// <summary>
		///     Whether to ignore <see cref="SymNamespaceAttribute.Namespace" /> if the enclosing type
		///     specifies a namespace.
		/// </summary>
		public bool IgnoreNamespace { get; set; }

		public SymImportAttribute() { }
	}
}