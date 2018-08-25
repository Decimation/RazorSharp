#region

using System;

#endregion

namespace RazorSharp.Utilities.Exceptions
{

	public class NotTranspiledException : NotImplementedException
	{
		public NotTranspiledException(string name) : base($"Sigcall method \"{name}\" has not been transpiled.") { }
		public NotTranspiledException() : base("Sigcall method has not been transpiled.") { }
	}

}