using SharpPdb.Windows;
using SharpPdb.Windows.SymbolRecords;

namespace RazorSharp.Native.Symbols
{
	public static class SymbolExt
	{
		
		
		public static string CleanName(this Public32Symbol symbol)
		{
			return NameUndecorator.UnDecorateSymbolName(symbol.Name, NameUndecorator.Flags.NameOnly);
		}
	}
}