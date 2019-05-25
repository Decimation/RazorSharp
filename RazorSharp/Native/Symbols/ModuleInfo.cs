using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SimpleSharp.Diagnostics;
using RazorSharp.CoreClr;
using RazorSharp.Memory;
using RazorSharp.Memory.Pointers;

//using SharpPdb.Windows;


namespace RazorSharp.Native.Symbols
{
	/// <summary>
	/// Combines a symbol with a process module.
	/// </summary>
	public class ModuleInfo
	{
		private readonly FileInfo m_pdb;

		public Pointer<byte> BaseAddress { get; }

		public ModuleInfo(FileInfo pdb, ProcessModule module) : this(pdb, module.BaseAddress) { }

		public ModuleInfo(FileInfo pdb, Pointer<byte> baseAddr)
		{
			Conditions.NotNull(baseAddr.Address, nameof(baseAddr));

			BaseAddress = baseAddr;
			m_pdb       = pdb;
		}

		public Symbol GetSymbol(string name)
		{
			SymbolManager.CurrentImage = m_pdb;
			return SymbolManager.GetSymbol(name);
		}

		public Pointer<byte> GetSymAddress(string name)
		{
			long ofs = GetSymbol(name).Offset;
			return BaseAddress + ofs;
		}

		public TDelegate GetFunction<TDelegate>(string name) where TDelegate : Delegate
		{
			return Functions.GetDelegateForFunctionPointer<TDelegate>(GetSymAddress(name));
		}

		public Pointer<byte>[] GetSymAddresses(string[] names)
		{
			SymbolManager.CurrentImage = m_pdb;
			var offsets = SymbolManager.GetSymOffsets(names);

			var rg = new Pointer<byte>[offsets.Length];

			for (int i = 0; i < rg.Length; i++) {
				rg[i] = BaseAddress + offsets[i];
			}

			return rg;
		}
	}
}