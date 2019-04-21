using System;
using System.IO;
using System.Linq;
using RazorSharp.Memory;
using RazorSharp.Memory.Pointers;
using SharpPdb.Windows;
using SharpPdb.Windows.SymbolRecords;

// ReSharper disable SuggestBaseTypeForParameter

namespace RazorSharp.Native.Symbols
{
	public class PdbSymbols : ISymbolResolver
	{
		private readonly PdbFile m_file;

		public PdbFile File => m_file;

		public PdbSymbols(FileInfo pdb)
		{
			m_file = new PdbFile(pdb.FullName);
		}

		public long GetSymOffset2(string name)
		{
			var sym = GetSymbol(name);

			return (long) sym.Offset;
		}

		public long GetSymOffset(string name)
		{
			var sym = GetSymbol(name);
			var rva = m_file.FindRelativeVirtualAddress(sym.Segment, sym.Offset);

			return (long) rva;
		}

		public long[] GetSymOffsets(string[] names)
		{
			var rg = new long[names.Length];

			for (int i = 0; i < rg.Length; i++) {
				rg[i] = GetSymOffset(names[i]);
			}

			return rg;
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
			if (name.Contains("::")) {
				var sz = name.Split(new[] {"::"}, StringSplitOptions.None);
				name = sz.Last();
			}


			var contains = m_file.PublicsStream.PublicSymbols.Where(s => s.Name.Contains(name)).ToArray();

			foreach (var symbol in contains) {
				if (symbol.CleanName() == name) {
					//Console.WriteLine("Choosing {0} {1} {2}", symbol.CleanName(), symbol.Flags, symbol.Kind);


					return symbol;
				}
			}


			return null;
		}
	}
}