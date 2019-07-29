

// ReSharper disable InconsistentNaming

namespace RazorSharp.Utilities.Security.Exceptions
{
	/// <summary>
	/// Describes an IL exception.
	/// </summary>
	internal sealed class CorILException : CoreException
	{
		[FailMessageTemplate]
		private const string ERR_MSG = "IL exception";

		public CorILException() : base() { }

		public CorILException(string message) : base(message) { }
	}
}