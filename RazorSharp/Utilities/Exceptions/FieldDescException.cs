#region

using System;

#endregion

namespace RazorSharp.Utilities.Exceptions
{

	internal class FieldDescException : Exception
	{
		public FieldDescException() { }
		public FieldDescException(string message) : base(message) { }
	}

}