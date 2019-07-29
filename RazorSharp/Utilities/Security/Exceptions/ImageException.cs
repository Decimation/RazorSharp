namespace RazorSharp.Utilities.Security.Exceptions
{
	/// <summary>
	/// Describes an exception with symbol/image handling.
	/// </summary>
	internal sealed class ImageException : CoreException
	{
		[FailMessageTemplate]
		private const string ERR_MSG = "Error loading image";

		public ImageException() : base() { }

		public ImageException(string msg) : base(msg) { }
	}
}