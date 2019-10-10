using RazorSharp.CoreClr.Meta.Base;

namespace RazorSharp.CoreClr.Meta.Base
{
	internal static class ClrStructureSettings
	{
		/// <summary>
		/// Prints <see cref="ClrStructure{T}.InfoTable"/>
		/// </summary>
		internal const string FORMAT_ALL = "A";

		/// <summary>
		/// Prints <see cref="ClrStructure{T}.IdTable"/>
		/// </summary>
		internal const string FORMAT_MIN = "M";

		internal static string DefaultFormat { get; set; } = FORMAT_MIN;
	}
}