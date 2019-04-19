#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using RazorCommon.Extensions;
using RazorSharp.Native.Win32;

#endregion

namespace RazorSharp.Native.Symbols
{
	public unsafe class SymbolEnvironment : ISymbolProvider
	{
		private ulong m_modBase;

		private string[] m_nameBuffer;

		private IntPtr m_proc;
		private Symbol m_symBuf;

		private List<Symbol> m_symBuffer;

		private bool m_isLoaded;

		private string m_img;

		private SymbolEnvironment() { }

		private static readonly SymbolEnvironment _instance;

		public static SymbolEnvironment Instance {
			[MethodImpl(MethodImplOptions.Synchronized)]
			get {
				lock (_instance) {
					return _instance;
				}
			}
		}


		static SymbolEnvironment()
		{
			_instance = new SymbolEnvironment();
		}

		public void Init(string img)
		{
			if (m_isLoaded) {
				Dispose();
			}

			m_img = img;

			m_proc = Kernel32.GetCurrentProcess();

			uint options = DbgHelp.SymGetOptions();

			// SYMOPT_DEBUG option asks DbgHelp to print additional troubleshooting
			// messages to debug output - use the debugger's Debug Output window
			// to view the messages


			options |= DbgHelp.SYMOPT_DEBUG;

			DbgHelp.SymSetOptions(options);

			// Initialize DbgHelp and load symbols for all modules of the current process 
			bool symInit = DbgHelp.SymInitialize(m_proc, null, true);

			NativeHelp.Call(symInit, nameof(DbgHelp.SymInitialize));

			ulong baseAddr = 0;
			ulong fileSize = 0;

			bool getFile = SymbolReader.GetFileParams(m_img, ref baseAddr, ref fileSize);
			NativeHelp.Call(getFile);

			m_modBase = DbgHelp.SymLoadModule64(
				m_proc,         // Process handle of the current process
				IntPtr.Zero,    // Handle to the module's image file (not needed)
				m_img,          // Path/name of the file
				null,           // User-defined short name of the module (it can be NULL)
				baseAddr,       // Base address of the module (cannot be NULL if .PDB file is used, otherwise it can be NULL)
				(uint) fileSize // Size of the file (cannot be NULL if .PDB file is used, otherwise it can be NULL)
			);

			NativeHelp.Call(m_modBase != 0, nameof(DbgHelp.SymLoadModule64));

			m_isLoaded = true;
		}


		public long GetSymOffset(string name)
		{
			return GetSymbol(name).Offset;
		}

		public long[] GetSymOffsets(string[] names)
		{
			Symbol[] sym = GetSymbols(names);
			int      lim = sym.Length;
			var      ofs = new long[lim];

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

			bool symEnumSuccess = DbgHelp.SymEnumSymbols(
				m_proc,             // Process handle of the current process
				m_modBase,          // Base address of the module
				null,               // Mask (NULL -> all symbols)
				CollectSymCallback, // The callback function
				IntPtr.Zero         // A used-defined context can be passed here, if necessary
			);

			NativeHelp.Call(symEnumSuccess, nameof(DbgHelp.SymEnumSymbols));


			Symbol[] cpy = m_symBuffer.OrderBy(s => names.IndexOf(s.Name)).ToArray();

			m_nameBuffer = null;
			m_symBuffer  = null;

			return cpy;
		}

		public Symbol GetSymbol(string name)
		{
			return GetSymbol(name, null);
		}

		public void Dispose()
		{
			Global.Log.Debug("Unloading symbols for image {Name}", m_img);

			NativeHelp.Call(DbgHelp.SymUnloadModule64(m_proc, m_modBase), nameof(DbgHelp.SymUnloadModule64));

			NativeHelp.Call(DbgHelp.SymCleanup(m_proc), nameof(DbgHelp.SymCleanup));

			m_proc     = IntPtr.Zero;
			m_symBuf   = null;
			m_isLoaded = false;
			m_img      = null;
		}

		private bool FirstSymCallback(IntPtr sym, uint symSize, IntPtr userCtx)
		{
			var pSym = (SymbolInfo*) sym;


			string symName = NativeHelp.GetString(&pSym->Name, pSym->NameLen);
			string ctxStr  = Marshal.PtrToStringAuto(userCtx);


			if (symName == ctxStr) {
				m_symBuf = new Symbol(pSym);
				return false;
			}

			return true;
		}


		private bool CollectSymCallback(IntPtr sym, uint symSize, IntPtr userCtx)
		{
			var    pSym    = (SymbolInfo*) sym;
			string symName = NativeHelp.GetString(&pSym->Name, pSym->NameLen);

			if (m_nameBuffer.Contains(symName)) {
				m_symBuffer.Add(new Symbol(pSym));
			}

			return m_symBuffer.Count != m_nameBuffer.Length;
		}

		public Symbol GetSymbol(string name, string mask)
		{
			var nameNative = Marshal.StringToHGlobalAuto(name);

			bool symEnumSuccess = DbgHelp.SymEnumSymbols(
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
	}
}