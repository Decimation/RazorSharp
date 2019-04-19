using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using RazorSharp.CoreClr;
using RazorSharp.Memory;
using RazorSharp.Memory.Pointers;
using SharpPdb.Windows;

namespace RazorSharp.Native.Symbols
{
	/// <summary>
	/// Combines a symbol file with a process module.
	/// </summary>
	public class ModuleInfo : IDisposable
	{
		private readonly PdbSymbols    m_reader;
		private readonly Pointer<byte> m_baseAddr;

		public ModuleInfo(FileInfo pdb, ProcessModule module) : this(pdb, module.BaseAddress) { }

		public ModuleInfo(FileInfo pdb, Pointer<byte> baseAddr)
		{
			m_reader = new PdbSymbols(pdb);
			m_baseAddr = baseAddr;
		}


		public Pointer<byte> GetSymAddress(string name)
		{
			long ofs = m_reader.GetSymbol(name).Offset;
			return m_baseAddr + ofs;
		}

		public TDelegate GetFunction<TDelegate>(string name) where TDelegate : Delegate
		{
			return Functions.GetDelegateForFunctionPointer<TDelegate>(GetSymAddress(name));
		}

		public Pointer<byte>[] GetSymAddresses(string[] names)
		{
			var offsets = m_reader.GetSymbols(names).Select(x => x.Offset).ToArray();

			var rg = new Pointer<byte>[offsets.Length];

			for (int i = 0; i < rg.Length; i++) {
				rg[i] = m_baseAddr + offsets[i];
			}

			return rg;
		}

		public void Dispose() { }
	}
}