using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using RazorCommon;
using RazorCommon.Diagnostics;
using RazorCommon.Extensions;
using RazorSharp.Native.Win32;

namespace RazorSharp.Native.Symbols
{
	public unsafe class SymbolEnvironment : ISymbolProvider
	{
		private string m_img;

		private IntPtr m_proc;
		private Symbol m_symBuf;

		private readonly ulong m_modBase;

		private List<Symbol> m_symBuffer;
		private string[]     m_nameBuffer;

		public SymbolEnvironment(string img)
		{
			m_img = img;

			m_proc = Kernel32.GetCurrentProcess();

			uint options = DbgHelp.SymGetOptions();

			// SYMOPT_DEBUG option asks DbgHelp to print additional troubleshooting
			// messages to debug output - use the debugger's Debug Output window
			// to view the messages


			options |= DbgHelp.SYMOPT_DEBUG;

			DbgHelp.SymSetOptions(options);

			// Initialize DbgHelp and load symbols for all modules of the current process 
			var symInit = DbgHelp.SymInitialize(m_proc, null, true);

			NativeHelp.Call(symInit, nameof(DbgHelp.SymInitialize));

			ulong baseAddr = 0;
			ulong fileSize = 0;

			bool getFile = SymbolReader.GetFileParams(img, ref baseAddr, ref fileSize);
			NativeHelp.Call(getFile);

			m_modBase = DbgHelp.SymLoadModule64(
				m_proc,         // Process handle of the current process
				IntPtr.Zero,    // Handle to the module's image file (not needed)
				img,            // Path/name of the file
				null,           // User-defined short name of the module (it can be NULL)
				baseAddr,       // Base address of the module (cannot be NULL if .PDB file is used, otherwise it can be NULL)
				(uint) fileSize // Size of the file (cannot be NULL if .PDB file is used, otherwise it can be NULL)
			);

			NativeHelp.Call(m_modBase != 0, nameof(DbgHelp.SymLoadModule64));
		}

		private bool FirstSymCallback(IntPtr sym, uint symSize, IntPtr userCtx)
		{
			var pSym = (SymbolInfo*) sym;


			var symName = NativeHelp.GetString(&pSym->Name, pSym->NameLen);
			var ctxStr  = Marshal.PtrToStringAuto(userCtx);


			if (symName == ctxStr) {
				m_symBuf = new Symbol(pSym);
				return false;
			}

			return true;
		}

		public long GetSymOffset(string name) => GetSymbol(name).Offset;

		public long[] GetSymOffsets(string[] names)
		{
			var sym = GetSymbols(names);
			var lim = sym.Length;
			var ofs = new long[lim];

//			Conditions.Ensure(sym.Length == lim, nameof(sym.Length));

			for (int i = 0; i < lim; i++) {
				ofs[i] = sym[i].Offset;
			}

			return ofs;
			
			//return GetSymbols(names).Select(x => x.Offset).Reverse().ToArray();
		}

		public Symbol[] GetSymbols(string[] names)
		{
			m_symBuffer  = new List<Symbol>(names.Length);
			m_nameBuffer = names;

			var symEnumSuccess = DbgHelp.SymEnumSymbols(
				m_proc,             // Process handle of the current process
				m_modBase,          // Base address of the module
				null,               // Mask (NULL -> all symbols)
				CollectSymCallback, // The callback function
				IntPtr.Zero         // A used-defined context can be passed here, if necessary
			);

			NativeHelp.Call(symEnumSuccess, nameof(DbgHelp.SymEnumSymbols));


			var cpy = m_symBuffer.OrderBy(s => names.IndexOf(s.Name)).ToArray();

			m_nameBuffer = null;
			m_symBuffer  = null;

			return cpy;
		}


		private bool CollectSymCallback(IntPtr sym, uint symSize, IntPtr userCtx)
		{
			var pSym = (SymbolInfo*) sym;
			var symName = NativeHelp.GetString(&pSym->Name, pSym->NameLen);

			if (m_nameBuffer.Contains(symName)) {
				m_symBuffer.Add(new Symbol(pSym));
			}

			return m_symBuffer.Count != m_nameBuffer.Length;
		}

		public Symbol GetSymbol(string name) => GetSymbol(name, null);
		
		public Symbol GetSymbol(string name, string mask)
		{
			var nameNative = Marshal.StringToHGlobalAuto(name);

			var symEnumSuccess = DbgHelp.SymEnumSymbols(
				m_proc,           // Process handle of the current process
				m_modBase,        // Base address of the module
				mask,             // Mask (NULL -> all symbols)
				FirstSymCallback, // The callback function
				nameNative        // A used-defined context can be passed here, if necessary
			);

			NativeHelp.Call(symEnumSuccess, nameof(DbgHelp.SymEnumSymbols));
			Marshal.FreeHGlobal(nameNative);


			var cpy = m_symBuf;
			m_symBuf = null;
			return cpy;
		}

		public void Dispose()
		{
			NativeHelp.Call(DbgHelp.SymUnloadModule64(m_proc, m_modBase), nameof(DbgHelp.SymUnloadModule64));

			NativeHelp.Call(DbgHelp.SymCleanup(m_proc), nameof(DbgHelp.SymCleanup));

			m_proc   = IntPtr.Zero;
			m_symBuf = null;
		}
	}
}