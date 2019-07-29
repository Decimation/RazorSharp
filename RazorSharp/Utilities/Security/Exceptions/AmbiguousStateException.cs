namespace RazorSharp.Utilities.Security.Exceptions
{
	/// <summary>
	/// Describes an exception in which the current program state is ambiguous.
	/// </summary>
	internal sealed class AmbiguousStateException : CoreException
	{
		[FailMessageTemplate]
		private const string ERR_MSG = "Ambiguous state exception";

		public AmbiguousStateException() : base() { }

		public AmbiguousStateException(string message) : base(message) { }
	}
}