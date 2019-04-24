using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using RazorSharp.Memory;
using RazorSharp.Memory.Pointers;
using RazorSharp.Native.Win32;

namespace RazorSharp.Native.Symbols
{
	public unsafe class SymbolManager : ISymbolResolver
	{
		private string m_img;
		private IntPtr m_proc;
		private ulong  m_modBase;
		
		public SymbolManager(FileInfo img) : this(img.FullName) { }

		public SymbolManager(string img)
		{
			m_img  = img;
			m_proc = Kernel32.GetCurrentProcess();

			uint options = DbgHelp.SymGetOptions();

			// SYMOPT_DEBUG option asks DbgHelp to print additional troubleshooting
			// messages to debug output - use the debugger's Debug Output window
			// to view the messages


			options |= DbgHelp.SYMOPT_DEBUG;

			DbgHelp.SymSetOptions(options);

			// Initialize DbgHelp and load symbols for all modules of the current process 
			bool symInit = DbgHelp.SymInitialize(m_proc, null, false);

			NativeHelp.Call(symInit, nameof(DbgHelp.SymInitialize));

			ulong baseAddr = 0;
			ulong fileSize = 0;

			bool getFile = SymbolAccess.GetFileParams(img, ref baseAddr, ref fileSize);
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


		public Symbol GetSymbol(string name)
		{
			int sz = (int) (Marshal.SizeOf<SymbolInfo>() + DbgHelp.MAX_SYM_NAME * sizeof(byte)
			                                             + sizeof(ulong) - 1 / sizeof(ulong));

			Pointer<SymbolInfo> buffer = stackalloc byte[sz];
			buffer.Reference.SizeOfStruct = (uint) Marshal.SizeOf<SymbolInfo>();
			buffer.Reference.MaxNameLen   = DbgHelp.MAX_SYM_NAME;


			if (DbgHelp.SymFromName(m_proc, name, buffer.Address)) {
				fixed (sbyte* firstChar = &buffer.Reference.Name) {
					var symName = NativeHelp.GetStringAlt(firstChar, (int) buffer.Reference.NameLen);
					return new Symbol(buffer.ToPointer<SymbolInfo>(), symName);
				}
			}

			throw new Exception(String.Format("Symbol \"{0}\" not found", name));
		}

		public Symbol[] GetSymbols(string[] names)
		{
			var rg = new Symbol[names.Length];

			for (int i = 0; i < rg.Length; i++) {
				rg[i] = GetSymbol(names[i]);
			}

			return rg;
		}

		public long GetSymOffset(string name)
		{
			return GetSymbol(name).Offset;
		}

		public long[] GetSymOffsets(string[] names)
		{
			return GetSymbols(names).Select(x => x.Offset).ToArray();
		}

		public void Dispose()
		{
			NativeHelp.Call(DbgHelp.SymUnloadModule64(m_proc, m_modBase), nameof(DbgHelp.SymUnloadModule64));
			NativeHelp.Call(DbgHelp.SymCleanup(m_proc), nameof(DbgHelp.SymCleanup));

			m_proc    = IntPtr.Zero;
			m_img     = null;
			m_modBase = 0;
		}
	}
}