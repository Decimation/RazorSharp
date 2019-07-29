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
	public class ImportForwardCallAttribute : ImportCallAttribute
	{
		public ImportForwardCallAttribute(Type type, string id, ImportCallOptions options)
		{
			Identifier = ImportManager.Combine(type.Name, id);
			Options = IdentifierOptions.FullyQualified;
			CallOptions = options;
		}
	}
}