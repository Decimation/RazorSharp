using System;

namespace RazorSharp.Utilities.Exceptions
{

	internal class MethodDescException : Exception
	{
		public MethodDescException() { }
		public MethodDescException(string message) : base(message) { }
	}

}