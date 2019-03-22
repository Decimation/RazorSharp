using System;

namespace RazorSharp.Utilities.Exceptions
{
	public class NativeException : Exception
	{
		public NativeException(string msg) : base(msg) { }
	}
}