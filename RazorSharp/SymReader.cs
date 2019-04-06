using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using RazorCommon.Diagnostics;
using RazorCommon.Utilities;
using RazorSharp.CoreClr;
using RazorSharp.Memory;
using RazorSharp.Native;
using RazorSharp.Native.Images;
using RazorSharp.Native.Symbols;
using RazorSharp.Native.Win32;
using RazorSharp.Pointers;
using static RazorSharp.Native.SymTagEnum;
using SymTagEnum = RazorSharp.Native.SymTagEnum;
using static RazorSharp.Native.Symbols.SymType;

namespace RazorSharp
{
	/// <summary>
	/// Provides access to symbols in PDB files and matching them with the corresponding memory
	/// location in an image
	/// </summary>
	internal unsafe class SymReader
	{
		// todo: add support for loading just one specified symbol like in Symbols
		
		
		internal List<Symbol> Symbols { get; private set; }

		private readonly IntPtr m_proc;

		private ulong m_modBase;

		private const string MASK_STR_DEFAULT = "*!*";

		private const string OffsetsFileName = "RazorSharpSymbolOffsets.txt";

		/// <summary>
		///     The file which caches symbols and their offsets
		/// </summary>
		private static readonly FileInfo CacheFile;

		/// <summary>
		///     <para>Key: Symbol name</para>
		///     <para>Value: Symbol offset</para>
		/// </summary>
		private static readonly Dictionary<string, long> Cache;
		
		
		static SymReader()
		{
			if (FileUtil.GetOrCreateTempFile(OffsetsFileName, out CacheFile)) {
				Global.Log.Verbose("Symbol cache file detected");
			}
			else {
				Global.Log.Verbose("Symbol cache file created");
			}

			Cache = FileUtil.ReadDictionary(CacheFile, s => s, Int64.Parse);

			Global.Log.Debug("Read {Count} cached offsets", Cache.Count);
		}
		
		/// <summary>
		///     Caches the offsets in <seealso cref="Cache" />
		/// </summary>
		public static void Close()
		{
			File.WriteAllText(CacheFile.FullName, String.Empty);
			Global.Log.Verbose("Writing {Count} offsets to cache", Cache.Count);
			FileUtil.WriteDictionary(Cache, CacheFile);
		}

		internal SymReader()
		{
			m_proc  = Kernel32.GetCurrentProcess();
			Symbols = new List<Symbol>();
		}
		
		public Symbol[] GetSymbols(string name)
		{
			var list = new List<Symbol>();
			
			foreach (var sym in Symbols) {
				if (sym.Name.Contains(name)) {
					list.Add(sym);
				}
			}

			return list.ToArray();
		}
		
		public Symbol GetSymbol(string name)
		{
			var symbols = GetSymbols(name);
			foreach (var symbol in symbols) {
				if (symbol.Name == name) {
					return symbol;
				}
			}


			var names       = symbols.Select(x => x.Name).ToList();
			var closestName = StringUtil.FindClosest(name, names);
			var closestSym  = symbols.First(sym => sym.Name == closestName);

			return closestSym;
		}
		
		public long GetSymOffset(string name)
		{
			if (Cache.ContainsKey(name)) {
//				Global.Log.Debug("Sym {Name} is cached", name);
				return Cache[name];
			}
			else {
				var ofs = GetSymbol(name).Offset;
				Cache.Add(name, ofs);
				return ofs;
			}
		}

		public long[] GetSymOffsets(string[] names)
		{
			var rg = new long[names.Length];

			for (int i = 0; i < names.Length; i++) {
				rg[i] = GetSymOffset(names[i]);
			}

			return rg;
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

			NativeFunctions.Call(bRet, nameof(DbgHelp.SymGetModuleInfo64));


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
		/// This has significant overhead
		/// </summary>
		internal void LoadAll(string pFileName, string pSearchMask)
		{
			uint options = DbgHelp.SymGetOptions();

			// SYMOPT_DEBUG option asks DbgHelp to print additional troubleshooting
			// messages to debug output - use the debugger's Debug Output window
			// to view the messages

			const uint SYMOPT_DEBUG = 0x80000000;
			options |= SYMOPT_DEBUG;

			DbgHelp.SymSetOptions(options);


			// Initialize DbgHelp and load symbols for all modules of the current process 
			var symInit = DbgHelp.SymInitialize(m_proc, null, true);

			NativeFunctions.Call(symInit, nameof(DbgHelp.SymInitialize));

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

			NativeFunctions.Call(m_modBase != 0, nameof(DbgHelp.SymLoadModule64));


			// Obtain and display information about loaded symbols 
			//SymGetModuleInfo(m_modBase);

			// Enumerate symbols and display information about them


			var symEnumSuccess = DbgHelp.SymEnumSymbols(
				m_proc,              // Process handle of the current process
				m_modBase,           // Base address of the module
				pSearchMask,         // Mask (NULL -> all symbols)
				EnumSymbolsCallback, // The callback function
				IntPtr.Zero          // A used-defined context can be passed here, if necessary
			);

			NativeFunctions.Call(symEnumSuccess, nameof(DbgHelp.SymEnumSymbols));

			NativeFunctions.Call(DbgHelp.SymUnloadModule64(m_proc, m_modBase), nameof(DbgHelp.SymUnloadModule64));

			NativeFunctions.Call(DbgHelp.SymCleanup(m_proc), nameof(DbgHelp.SymCleanup));

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

		private static bool GetFileParams(string pFileName, ref ulong baseAddr, ref ulong fileSize)
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

		#region Download

		internal static FileInfo DownloadSymbolFile(DirectoryInfo dest, FileInfo dll)
        		{
        			return DownloadSymbolFile(dest, dll, out _);
        		}
        
        		internal static FileInfo DownloadSymbolFile(DirectoryInfo dest, FileInfo dll, out DirectoryInfo super)
        		{
        			// symchk
        			string progFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        			var    symChk    = new FileInfo(String.Format(@"{0}\Windows Kits\10\Debuggers\x64\symchk.exe", progFiles));
        			Conditions.Require(symChk.Exists);
        
        			string cmd = String.Format("\"{0}\" \"{1}\" /s SRV*{2}*http://msdl.microsoft.com/download/symbols",
        			                           symChk.FullName, dll.FullName, dest.FullName);
        
        
        			using (var cmdProc = Common.Shell("\"" + cmd + "\"")) {
        				var startTime = DateTimeOffset.Now;
        
        				cmdProc.ErrorDataReceived += (sender, args) =>
        				{
        					Global.Log.Error("Process error: {Error}", args.Data);
        				};
        
        				cmdProc.Start();
        
        				var stdOut = cmdProc.StandardOutput;
        				while (!stdOut.EndOfStream) {
        					string ln = stdOut.ReadLine();
        					Conditions.NotNull(ln, nameof(ln));
        					if (ln.Contains("SYMCHK: PASSED + IGNORED files = 1")) {
        						break;
        					}
        
        					if (DateTimeOffset.Now.Subtract(startTime).TotalMinutes > 1.5) {
        						throw new TimeoutException("Could not download CLR symbols");
        					}
        				}
        			}
        
        			Global.Log.Debug("Done downloading symbols");
        
        			string   pdbStr = dest.FullName + @"\" + Clr.CLR_PDB_SHORT;
        			FileInfo pdb;
        
        			if (Directory.Exists(pdbStr)) {
        				// directory will be named <symbol>.pdb
        				super = new DirectoryInfo(pdbStr);
        
        				// sole child directory will be something like 9FF14BF5D36043909E88FF823F35EE3B2
        				DirectoryInfo[] children = super.GetDirectories();
        				Conditions.Assert(children.Length == 1);
        				var child = children[0];
        
        				// (possibly sole) file will be the symbol file
        				FileInfo[] files = child.GetFiles();
        				pdb = files.First(f => f.Name.Contains(Clr.CLR_PDB_SHORT));
        			}
        			else if (File.Exists(pdbStr)) {
        				super = null;
        				pdb   = new FileInfo(pdbStr);
        			}
        			else {
        				throw new Exception(String.Format("Error downloading symbols. File: {0}", pdbStr));
        			}
        
        			Conditions.Ensure(pdb.Exists);
        			return pdb;
        		}

		#endregion
	}
}