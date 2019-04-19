using System;
using System.IO;
using System.Linq;
using RazorSharp.Memory.Pointers;
using SharpPdb.Windows;
using SharpPdb.Windows.SymbolRecords;

// ReSharper disable SuggestBaseTypeForParameter

namespace RazorSharp.Native.Symbols
{
	public class PdbSymbols : ISymbolResolver
	{
		private readonly PdbFile m_file;

		public PdbSymbols(FileInfo pdb)
		{
			m_file = new PdbFile(pdb.FullName);
		}

		public long   GetSymOffset(string    name)  => GetSymbol(name).Offset;
		public long[] GetSymOffsets(string[] names) => GetSymbols(names).Select(x => (long) x.Offset).ToArray();

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
			if (name.Contains("::")) {
				var sz = name.Split(new string[] {"::"}, StringSplitOptions.None);
				name = sz.Last();
			}

			foreach (var symbol in m_file.PublicsStream.PublicSymbols) {
//				if (SymbolAccess.Undname(symbol.Name).Contains(name)) {
//					return symbol;
//				}
				if (symbol.Name.Contains(name))
					return symbol;
			}

			return null;
		}
	}
}