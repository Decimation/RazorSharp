using System;
using System.IO;
using RazorSharp.Memory.Pointers;
using SharpPdb.Windows;
using SharpPdb.Windows.SymbolRecords;
// ReSharper disable SuggestBaseTypeForParameter

namespace RazorSharp.Native.Symbols
{
	public class PdbSymbols
	{
		private readonly PdbFile m_file;

		public PdbSymbols(FileInfo pdb)
		{
			m_file = new PdbFile(pdb.FullName);
		}

		public Public32Symbol[] GetSymbols(string[] names)
		{
			var rg = new Public32Symbol[names.Length];
			
			for (int i = 0; i < rg.Length; i++) {
				rg[i] = GetSymbol(names[i]);
			}

			return rg;
		}
		
		public Public32Symbol GetSymbol(string name)
		{
			foreach (var symbol in m_file.PublicsStream.PublicSymbols) {
				if (SymbolAccess.Undname(symbol.Name).Contains(name)) {
					return symbol;
				}
			}

			return null;
		}
	}
}