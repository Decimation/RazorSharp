#region

using System;

#endregion

namespace RazorSharp.Utilities.Exceptions
{

	internal class SigcallException : NotImplementedException
	{
		public SigcallException(string name) : base($"Sigcall method \"{name}\" has not been bound.") { }
		public SigcallException() : base("Sigcall method has not been bound.") { }
	}

}