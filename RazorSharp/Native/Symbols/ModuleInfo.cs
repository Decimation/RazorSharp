using System;
using System.Diagnostics;
using System.IO;
using RazorSharp.Memory;
using RazorSharp.Memory.Pointers;

namespace RazorSharp.Native.Symbols
{
	/// <summary>
	/// Combines a symbol file with a process module.
	/// </summary>
	public class ModuleInfo : IDisposable
	{
		private readonly SymbolEnvironment m_reader;
		private readonly Pointer<byte> m_baseAddr;

		public ModuleInfo(FileInfo pdb, ProcessModule module) : this(pdb, module.BaseAddress)
		{
			
		}
		
		public ModuleInfo(FileInfo pdb, Pointer<byte> baseAddr)
		{
			m_reader = new SymbolEnvironment(pdb.FullName);
			m_baseAddr = baseAddr;
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
			m_reader.Dispose();
		}
	}
}