#region

using System;

#endregion

namespace RazorSharp.Utilities.Exceptions
{

	public class NotTranspiledException : NotImplementedException
	{
		public NotTranspiledException() : base("Sigcall method has not been transpiled.") { }
	}

}