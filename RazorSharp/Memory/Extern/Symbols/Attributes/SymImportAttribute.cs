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
		///     Name of the symbol. If this is <c>null</c>, the member name will be used instead.
		/// </summary>
		public string Symbol { get; set; }

		public SymImportOptions Options { get; set; }

		public SymImportAttribute() : this(SymImportOptions.None) { }

		public SymImportAttribute(SymImportOptions options) : this(null, options) { }

		public SymImportAttribute(string symbol, SymImportOptions options = SymImportOptions.None)
		{
			Symbol  = symbol;
			Options = options;
		}
	}
}