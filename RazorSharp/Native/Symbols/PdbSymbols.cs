

//using SharpPdb.Windows;
//using SharpPdb.Windows.SymbolRecords;

// ReSharper disable SuggestBaseTypeForParameter

namespace RazorSharp.Native.Symbols
{
	/*public class PdbSymbols : ISymbolResolver
	{
		public PdbFile File { get; }

		public PdbSymbols(FileInfo pdb)
		{
			File = new PdbFile(pdb.FullName);
		}

		public long GetSymOffset(string name)
		{
			var sym = GetSymbol(name);
			var rva = File.FindRelativeVirtualAddress(sym.Segment, sym.Offset);

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
			if (name.Contains(Symload.SCOPE_RESOLUTION_OPERATOR)) {
				var sz = name.Split(new[] {Symload.SCOPE_RESOLUTION_OPERATOR}, StringSplitOptions.None);
				name = sz.Last();
			}


			var contains = File.PublicsStream.PublicSymbols.Where(s => s.Name.Contains(name)).ToArray();

			foreach (var symbol in contains) {
				if (symbol.CleanName() == name) {
					//Console.WriteLine("Choosing {0} {1} {2}", symbol.CleanName(), symbol.Flags, symbol.Kind);
					return symbol;
				}
			}


			return null;
		}

		public void Dispose()
		{
			File.Dispose();
		}
	}*/
}