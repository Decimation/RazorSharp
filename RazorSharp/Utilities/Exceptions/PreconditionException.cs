#region

using System;

#endregion

namespace RazorSharp.Utilities.Exceptions
{

	internal class PreconditionException : Exception
	{
		private const string MSG_PRECONDITION_FAIL = "Precondition failed";

		public PreconditionException() : this(MSG_PRECONDITION_FAIL) { }
		internal PreconditionException(string msg) : base(msg) { }
	}

}