using System;

namespace RazorSharp.Native.Symbols
{
	public interface ISymbolResolver : IDisposable
	{
		long GetSymOffset(string name);
		long[] GetSymOffsets(string[] names);
	}
}