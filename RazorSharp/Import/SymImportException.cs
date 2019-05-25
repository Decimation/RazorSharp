#region

using System;

#endregion

namespace RazorSharp.Import
{
	public class SymImportException : NotImplementedException
	{
		public SymImportException(string name) : base($"Symbol import \"{name}\" error") { }

//		public SymImportException() : base("Symbol import error") { }

		public SymImportException(string name, string msg)
			: base($"Symbol import \"{name}\" error: \"{msg}\"") { }
	}
}