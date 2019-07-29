using System;

namespace RazorSharp.Utilities.Security.Exceptions
{
	
	internal abstract class CoreException : Exception
	{
		protected CoreException() : base() { }

		protected CoreException(string message) : base(message) { }
	}
}