namespace RazorSharp.Utilities.Security.Exceptions
{
	/// <summary>
	/// Describes an exception within the CLR.
	/// </summary>
	internal sealed class ClrException : CoreException
	{
		[FailMessageTemplate]
		private const string ERR_MSG = "CLR exception";

		public ClrException() : base() { }

		public ClrException(string message) : base(message) { }
	}
}