#region

using System;

#endregion

namespace RazorSharp.Utilities.Exceptions
{
	/// <inheritdoc />
	/// <summary>
	///     CLR-related exception
	/// </summary>
	internal class RuntimeException : Exception
	{
		public RuntimeException() { }
		public RuntimeException(string msg) : base(msg) { }
	}
}