#region

using System;
using System.Diagnostics;

#endregion

namespace RazorSharp.Diagnostics.Exceptions
{
	public class SigcallException : NotImplementedException
	{
		public SigcallException(string name) : base($"Sigcall method \"{name}\" error") { }

		public SigcallException() : base("Sigcall method error")
		{
			var method = new StackTrace().GetFrame(1).GetMethod();
		}
	}
}