using System;

namespace RazorSharp.Memory.Extern.Symbols.Attributes
{
	[AttributeUsage(AttributeTargets.Field)]
	public class SymFieldAttribute : SymImportAttribute
	{
		/// <summary>
		/// The <see cref="Type"/> to load this field as. If left unset, the field will be interpreted as
		/// the target field's type.
		/// </summary>
		public Type LoadAs { get; set; }

		public SymFieldAttribute() : base() { }

		public SymFieldAttribute(SymImportOptions options) : base(options) { }

		public SymFieldAttribute(string symbol, SymImportOptions options = SymImportOptions.None)
			: base(symbol, options) { }
	}
}