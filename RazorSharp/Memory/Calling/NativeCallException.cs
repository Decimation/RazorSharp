#region

using System;

#endregion

namespace RazorSharp.Memory.Calling
{
	public class NativeCallException : NotImplementedException
	{
		public NativeCallException(string name) : base($"Symcall / sigcall native method \"{name}\" error") { }

		public NativeCallException() : base("Symcall / sigcall native method error") { }
	}
}