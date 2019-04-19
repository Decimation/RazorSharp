namespace RazorSharp.Native.Symbols
{
	public interface ISymbolResolver
	{
		long GetSymOffset(string name);
		long[] GetSymOffsets(string[] names);
	}
}