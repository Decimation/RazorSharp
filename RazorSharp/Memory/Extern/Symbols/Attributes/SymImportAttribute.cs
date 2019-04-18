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
		/// <summary>
		///     Name of the symbol
		/// </summary>
		public string Symbol { get; set; }

		/// <summary>
		///     Debugging symbol file (PDB, etc)
		/// </summary>
		public string Image { get; set; }

		/// <summary>
		///     <see cref="ProcessModule" /> from which to calculate the function address
		/// </summary>
		public string Module { get; set; }

		/// <summary>
		///     Whether <see cref="Symbol" /> is the fully qualified name
		/// </summary>
		public bool FullyQualified { get; set; }

		/// <summary>
		///     Whether to use the decorated method's name as the symbol.
		/// </summary>
		public bool UseMethodNameOnly { get; set; }

		/// <summary>
		///     Whether to ignore <see cref="SymNamespaceAttribute.Namespace" /> if the enclosing type
		///     specifies a namespace.
		/// </summary>
		public bool IgnoreNamespace { get; set; }
	}
}