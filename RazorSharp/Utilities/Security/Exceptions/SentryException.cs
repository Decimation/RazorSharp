namespace RazorSharp.Utilities.Security.Exceptions
{
	/// <summary>
	/// Thrown by <see cref="Guard"/>
	/// </summary>
	internal class SentryException : CoreException
	{
		[FailMessageTemplate]
		private const string ERR_MSG = "Sentry exception";

		public SentryException() : base() { }

		public SentryException(string message) : base(message) { }
	}
}