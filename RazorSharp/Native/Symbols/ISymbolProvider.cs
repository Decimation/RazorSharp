#region

using System;

#endregion

namespace RazorSharp.Native.Symbols
{
	public interface ISymbolProvider : IDisposable
	{
		long[] GetSymOffsets(string[] names);
		long   GetSymOffset(string    name);

		Symbol[] GetSymbols(string[] names);
		Symbol   GetSymbol(string    name);
	}
}