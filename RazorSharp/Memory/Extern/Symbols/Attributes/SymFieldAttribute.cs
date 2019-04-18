using System;

namespace RazorSharp.Memory.Extern.Symbols.Attributes
{
	[AttributeUsage(AttributeTargets.Field)]
	public class SymFieldAttribute : SymImportAttribute
	{
		public Type LoadAs { get; set; }

		public SymFieldAttribute() : base() {}
	}
}