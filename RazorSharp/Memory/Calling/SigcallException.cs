#region

using System;

#endregion

namespace RazorSharp.Memory.Calling
{
	public class SigcallException : NotImplementedException
	{
		public SigcallException(string name) : base($"Sigcall method \"{name}\" error") { }

		public SigcallException() : base("Sigcall method error")
		{
			
		}
	}
}