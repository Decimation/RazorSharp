using System;

namespace RazorSharp.Utilities
{

	internal class RuntimeException : Exception
	{
		internal RuntimeException(string msg) : base(msg)
		{

		}
	}

}