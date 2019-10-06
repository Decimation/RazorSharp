using System;
using JetBrains.Annotations;
using RazorSharp.Import.Enums;

namespace RazorSharp.Import.Attributes
{
	/// <summary>
	/// Used when the function is not resolved from its declaring type -- that is, its identifier is resolved
	/// from another type.
	/// </summary>
	[MeansImplicitUse]
	[AttributeUsage(METHOD_TARGETS)]
	public sealed class ImportForwardCallAttribute : ImportCallAttribute
	{
		public ImportForwardCallAttribute(string nameSpace, string id, ImportCallOptions options)
		{
			Identifier  = ImportManager.Combine(nameSpace, id);
			Options     = IdentifierOptions.FullyQualified;
			CallOptions = options;
		}

		public ImportForwardCallAttribute(Type type, string id, ImportCallOptions options)
			: this(type.Name, id, options) { }
	}
}