using RazorSharp.Core;
using RazorSharp.Import;
using RazorSharp.Import.Attributes;

namespace RazorSharp.Interop
{
	/// <summary>
	/// Provides methods for creating and modifying functions.
	/// </summary>
	[ImportNamespace]
	public static unsafe partial class FunctionFactory
	{
		static FunctionFactory()
		{
			ImportManager.Value.Load(typeof(FunctionFactory), Clr.Value.Imports);
		}

		[ImportMapDesignation]
		private static readonly ImportMap Imports = new ImportMap();
	}
}