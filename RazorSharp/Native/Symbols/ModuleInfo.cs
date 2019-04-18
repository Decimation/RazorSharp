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
		private readonly ProcessModule     m_module;

		public ModuleInfo(FileInfo pdb, ProcessModule module)
		{
			m_reader = new SymbolEnvironment(pdb.FullName);
			m_module = module;
		}

		public Pointer<byte> GetSymAddress(string name)
		{
			long ofs = m_reader.GetSymOffset(name);
			return Modules.GetAddress(m_module, ofs);
		}

		public TDelegate GetFunction<TDelegate>(string name) where TDelegate : Delegate
		{
			return Functions.GetDelegateForFunctionPointer<TDelegate>(GetSymAddress(name));
		}

		public Pointer<byte>[] GetSymAddresses(string[] names)
		{
			var offsets = m_reader.GetSymOffsets(names);
			return Modules.GetAddresses(m_module, offsets);
		}

		public void Dispose()
		{
			m_reader.Dispose();
		}
	}
}