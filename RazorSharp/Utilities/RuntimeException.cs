#region

using System;

#endregion

namespace RazorSharp.Utilities
{

	internal class RuntimeException : Exception
	{
		internal RuntimeException(string msg) : base(msg) { }
	}

}