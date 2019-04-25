#region

using System;

#endregion

namespace RazorSharp.Memory.Extern
{
	public class SymImportException : NotImplementedException
	{
		public SymImportException(string name) : base($"Symbol import \"{name}\" error") { }

		public SymImportException() : base("Symbol import error") { }
	}
}