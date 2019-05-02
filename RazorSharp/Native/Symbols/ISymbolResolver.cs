#region

using System;

#endregion

namespace RazorSharp.Native.Symbols
{
	public interface ISymbolResolver : IDisposable
	{
		long   GetSymOffset(string    name);
		long[] GetSymOffsets(string[] names);
	}
}