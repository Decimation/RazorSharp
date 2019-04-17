#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using RazorCommon.Diagnostics;
using RazorCommon.Utilities;
using RazorSharp.CoreClr;
using RazorSharp.Memory;
using RazorSharp.Memory.Pointers;
using RazorSharp.Native.Images;
using RazorSharp.Native.Win32;

#endregion

namespace RazorSharp.Native.Symbols
{
	/// <summary>
	///     Provides access to symbols in PDB files and matching them with the corresponding memory
	///     location in an image
	/// </summary>
	public unsafe class SymbolReader : ISymbolProvider
	{
		private const string MASK_STR_DEFAULT = "*!*";

		private const string OFFSETS_FILE_NAME = "RazorSharpSymbolOffsets.txt";

		/// <summary>
		///     The file which caches symbols and their offsets
		/// </summary>
		private static readonly FileInfo CacheFile;

		/// <summary>
		///     <para>Key: Symbol name</para>
		///     <para>Value: Symbol offset</para>
		/// </summary>
		private static readonly Dictionary<string, long> Cache;

		private readonly IntPtr m_proc;

		private ulong m_modBase;


		static SymbolReader()
		{
			if (FileUtil.GetOrCreateTempFile(OFFSETS_FILE_NAME, out CacheFile)) {
				Global.Log.Verbose("Symbol cache file detected");
			}
			else {
				Global.Log.Verbose("Symbol cache file created");
			}

			Cache = FileUtil.ReadDictionary(CacheFile, s => s, Int64.Parse);

			Global.Log.Debug("Read {Count} cached offsets", Cache.Count);
		}

		public SymbolReader()
		{
			m_proc  = Kernel32.GetCurrentProcess();
			Symbols = new List<Symbol>();
		}

		internal List<Symbol> Symbols { get; }

		public Symbol[] GetSymbols(string[] names)
		{
			throw new NotImplementedException();
		}

		public Symbol GetSymbol(string name)
		{
			Symbol[] symbols = GetSymbolsContainingName(name);
			
			if (symbols.Length == 0) {
				return null;
			}
			
			foreach (var symbol in symbols) {
				if (symbol.Name == name) {
					return symbol;
				}
			}


			

			List<string> names       = symbols.Select(x => x.Name).ToList();
			string       closestName = StringUtil.FindClosest(name, names);
			var          closestSym  = symbols.First(sym => sym.Name == closestName);

			return closestSym;
		}

		public long GetSymOffset(string name)
		{
			if (Cache.ContainsKey(name)) {
//				Global.Log.Debug("Sym {Name} is cached", name);
				return Cache[name];
			}

			long ofs = GetSymbol(name).Offset;
			Cache.Add(name, ofs);
			return ofs;
		}

		public long[] GetSymOffsets(string[] names)
		{
			var rg = new long[names.Length];

			for (int i = 0; i < names.Length; i++) {
				rg[i] = GetSymOffset(names[i]);
			}

			return rg;
		}

		public void Dispose() { }

		/// <summary>
		///     Caches the offsets in <seealso cref="Cache" />
		/// </summary>
		public static void Close()
		{
			File.WriteAllText(CacheFile.FullName, String.Empty);
			Global.Log.Verbose("Writing {Count} offsets to cache", Cache.Count);
			FileUtil.WriteDictionary(Cache, CacheFile);
		}

		public Symbol[] GetSymbolsContainingName(string name)
		{
			Conditions.Require(Symbols.Count > 0, "Symbols have not been loaded", null);

			var list = new List<Symbol>();
			
			foreach (var sym in Symbols) {
				if (sym.Name.Contains(name)) {
					list.Add(sym);
				}
			}

			return list.ToArray();
		}

		public Pointer<byte> GetSymAddress(string name, string module)
		{
			long offset = GetSymOffset(name);
			return Modules.GetAddress(module, offset);
		}

		public Pointer<byte> GetClrSymAddress(string name)
		{
			return GetSymAddress(name, Clr.CLR_DLL_SHORT);
		}

		#region Retrieval

		private void SymGetModuleInfo(ulong modBase)
		{
			ImageHelpModule64 module64 = default;

			int size = Marshal.SizeOf<ImageHelpModule64>();


			module64.SizeOfStruct = size;

			var pAlloc = Marshal.AllocHGlobal(size);
			Mem.Zero(pAlloc, size);
			Marshal.StructureToPtr(module64, pAlloc, false);

			bool bRet = DbgHelp.SymGetModuleInfo64(m_proc, modBase, pAlloc);

			NativeHelp.Call(bRet, nameof(DbgHelp.SymGetModuleInfo64));


			module64 = Marshal.PtrToStructure<ImageHelpModule64>(pAlloc);


			Marshal.FreeHGlobal(pAlloc);


			// Is debug information unmatched ?
			// (It can only happen if the debug information is contained
			// in a separate file (.DBG or .PDB)

			if (module64.PdbUnmatched || module64.DbgUnmatched) { }
		}

		private bool EnumSymbolsCallback(IntPtr pSymInfo, uint symSize, IntPtr userCtx)
		{
			if (pSymInfo != IntPtr.Zero) {
				Symbols.Add(new Symbol((SymbolInfo*) pSymInfo));
			}

			return true; // continue enumeration
		}

		/// <summary>
		///     This has significant overhead
		/// </summary>
		public void LoadAll(string pFileName, string pSearchMask)
		{
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

			bool getFile = GetFileParams(pFileName, ref baseAddr, ref fileSize);
			Conditions.Require(getFile, nameof(getFile));


			m_modBase = DbgHelp.SymLoadModule64(
				m_proc,         // Process handle of the current process
				IntPtr.Zero,    // Handle to the module's image file (not needed)
				pFileName,      // Path/name of the file
				null,           // User-defined short name of the module (it can be NULL)
				baseAddr,       // Base address of the module (cannot be NULL if .PDB file is used, otherwise it can be NULL)
				(uint) fileSize // Size of the file (cannot be NULL if .PDB file is used, otherwise it can be NULL)
			);

			NativeHelp.Call(m_modBase != 0, nameof(DbgHelp.SymLoadModule64));


			// Obtain and display information about loaded symbols 
			//SymGetModuleInfo(m_modBase);

			// Enumerate symbols and display information about them


			bool symEnumSuccess = DbgHelp.SymEnumSymbols(
				m_proc,              // Process handle of the current process
				m_modBase,           // Base address of the module
				pSearchMask,         // Mask (NULL -> all symbols)
				EnumSymbolsCallback, // The callback function
				IntPtr.Zero          // A used-defined context can be passed here, if necessary
			);

			NativeHelp.Call(symEnumSuccess, nameof(DbgHelp.SymEnumSymbols));

			NativeHelp.Call(DbgHelp.SymUnloadModule64(m_proc, m_modBase), nameof(DbgHelp.SymUnloadModule64));

			NativeHelp.Call(DbgHelp.SymCleanup(m_proc), nameof(DbgHelp.SymCleanup));

			// Complete
		}


		private static bool GetFileSize(string pFileName, ref ulong fileSize)
		{
			var hFile = Kernel32.CreateFile(pFileName,
			                                FileAccess.Read,
			                                FileShare.Read,
			                                IntPtr.Zero,
			                                FileMode.Open,
			                                0,
			                                IntPtr.Zero);


			if (hFile == Kernel32.INVALID_HANDLE_VALUE) {
				return false;
			}

			fileSize = Kernel32.GetFileSize(hFile, null);

			Conditions.Ensure(Kernel32.CloseHandle(hFile));

			if (fileSize == Kernel32.INVALID_FILE_SIZE) {
				return false;
			}


			return fileSize != Kernel32.INVALID_FILE_SIZE;
		}

		// ReSharper disable once RedundantAssignment
		internal static bool GetFileParams(string pFileName, ref ulong baseAddr, ref ulong fileSize)
		{
			// Is it .PDB file ?

			if (pFileName.Contains("pdb")) {
				// Yes, it is a .PDB file 

				// Determine its size, and use a dummy base address 

				baseAddr = 0x10000000; // it can be any non-zero value, but if we load symbols 
				// from more than one file, memory regions specified
				// for different files should not overlap
				// (region is "base address + file size")

				if (!GetFileSize(pFileName, ref fileSize)) {
					return false;
				}
			}
			else {
				// It is not a .PDB file 

				// Base address and file size can be 0 

				baseAddr = 0;
				fileSize = 0;

				throw new NotImplementedException();
			}

			return true;
		}

		#endregion

	}
}