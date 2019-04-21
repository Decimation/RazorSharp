using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using RazorCommon.Diagnostics;
using RazorSharp.CoreClr;
using RazorSharp.Memory;
using RazorSharp.Memory.Pointers;
using SharpPdb.Windows;

namespace RazorSharp.Native.Symbols
{
	public enum SymbolRetrievalMode
	{
		Kernel,
		PdbReader
	}

	/// <summary>
	/// Combines a symbol file with a process module.
	/// </summary>
	public class ModuleInfo : IDisposable
	{
		private readonly ISymbolResolver     m_reader;
		private readonly Pointer<byte>       m_baseAddr;
		private readonly SymbolRetrievalMode m_mode;

		public Pointer<byte> BaseAddress => m_baseAddr;

		public ModuleInfo(FileInfo pdb, ProcessModule module, SymbolRetrievalMode mode)
			: this(pdb, module.BaseAddress, mode) { }

		public ModuleInfo(FileInfo pdb, Pointer<byte> baseAddr, SymbolRetrievalMode mode)
		{
			Conditions.NotNull(baseAddr.Address, nameof(baseAddr));
			
			m_baseAddr = baseAddr;
			m_mode     = mode;

			switch (m_mode) {
				case SymbolRetrievalMode.Kernel:
					m_reader = SymbolEnvironment.Instance;
					((SymbolEnvironment) m_reader).Init(pdb.FullName);
					break;
				case SymbolRetrievalMode.PdbReader:
					m_reader = new PdbSymbols(pdb);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}


		public Pointer<byte> GetSymAddress(string name)
		{
			long ofs = m_reader.GetSymOffset(name);
			return m_baseAddr + ofs;
		}

		public TDelegate GetFunction<TDelegate>(string name) where TDelegate : Delegate
		{
			return Functions.GetDelegateForFunctionPointer<TDelegate>(GetSymAddress(name));
		}

		public Pointer<byte>[] GetSymAddresses(string[] names)
		{
			var offsets = m_reader.GetSymOffsets(names);

			var rg = new Pointer<byte>[offsets.Length];

			for (int i = 0; i < rg.Length; i++) {
				rg[i] = m_baseAddr + offsets[i];
			}

			return rg;
		}

		public void Dispose()
		{
			if (m_mode == SymbolRetrievalMode.Kernel)
				((SymbolEnvironment) m_reader).Dispose();
		}
	}
}