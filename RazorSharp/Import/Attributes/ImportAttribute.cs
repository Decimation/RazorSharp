using System;
using JetBrains.Annotations;
using RazorSharp.Import.Enums;

namespace RazorSharp.Import.Attributes
{
	[MeansImplicitUse]
	[AttributeUsage(ImportFieldAttribute.FIELD_TARGETS | ImportCallAttribute.METHOD_TARGETS)]
	public abstract class ImportAttribute : Attribute
	{
		public string Identifier { get; set; }

		public IdentifierOptions Options { get; set; }

		public ImportAttribute() : this(IdentifierOptions.None) { }

		public ImportAttribute(IdentifierOptions options) : this(null, options) { }

		public ImportAttribute(string id, IdentifierOptions options = IdentifierOptions.None)
		{
			Identifier = id;
			Options    = options;
		}
	}
}