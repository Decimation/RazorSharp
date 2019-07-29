using RazorSharp.Import;

namespace RazorSharp.Utilities.Security.Exceptions
{
	/// <summary>
	/// Describes an exception with <see cref="ImportManager"/>
	/// </summary>
	internal sealed class ImportException : CoreException
	{
		[FailMessageTemplate]
		private const string ERR_MSG = "Import exception";

		public ImportException() : base() { }

		public ImportException(string message) : base(message) { }
	}
}