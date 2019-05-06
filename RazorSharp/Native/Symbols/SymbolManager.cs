using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using RazorSharp.Memory.Pointers;
using RazorSharp.Native.Win32;
using SimpleSharp.Diagnostics;

// ReSharper disable ReturnTypeCanBeEnumerable.Global

namespace RazorSharp.Native.Symbols
{
	/// <summary>
	/// Provides access to symbols in a specified image
	/// </summary>
	internal static unsafe class SymbolManager
	{
		private static IntPtr _proc;
		private static ulong  _modBase;

//		private static string[] _nameBuffer;
		private static string   _singleNameBuffer;

		private static List<Symbol> _symBuffer;

		/// <summary>
		/// Current image
		/// </summary>
		private static FileInfo _pdb;

		internal static bool IsSetup { get; private set; }


		internal static bool IsImageLoaded {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _modBase != 0;
		}

		internal static FileInfo CurrentImage {
			get => _pdb;
			set {
				if (_pdb != value) {
					_pdb = value;
					Load();
				}
			}
		}

		internal static void Close()
		{
			Conditions.Require(IsSetup, nameof(IsSetup));
			UnloadModule();
			NativeHelp.Call(DbgHelp.SymCleanup(_proc), nameof(DbgHelp.SymCleanup));

			_proc    = IntPtr.Zero;
			_modBase = 0;
			_pdb     = null;

			ClearBuffer();

			IsSetup = false;
		}

		internal static void Setup()
		{
			_proc = Kernel32.GetCurrentProcess();

			uint options = DbgHelp.SymGetOptions();

			// SYMOPT_DEBUG option asks DbgHelp to print additional troubleshooting
			// messages to debug output - use the debugger's Debug Output window
			// to view the messages


			options |= DbgHelp.SYMOPT_DEBUG;

			DbgHelp.SymSetOptions(options);

			// Initialize DbgHelp and load symbols for all modules of the current process 
			bool symInit = DbgHelp.SymInitialize(_proc, null, false);

			NativeHelp.Call(symInit, nameof(DbgHelp.SymInitialize));

			IsSetup = true;
		}

		private static void UnloadModule()
		{
			if (IsImageLoaded) {
				Global.Log.Verbose("Unloading image {Img}", _pdb.Name);
				NativeHelp.Call(DbgHelp.SymUnloadModule64(_proc, _modBase), nameof(DbgHelp.SymUnloadModule64));
			}
		}

		private static void CheckModule()
		{
			if (!IsImageLoaded) {
				string msg =
					$"Error loading image. This may be an error with {nameof(Load)}. Have you loaded an image?";

				throw new Exception(msg);
			}
		}


		private static void Load()
		{
			string img = _pdb.FullName;

			Global.Log.Verbose("Loading image {Img}", _pdb.Name);

			UnloadModule();
			Conditions.Require(IsSetup, nameof(IsSetup));

			ulong baseAddr = 0;
			ulong fileSize = 0;

			bool getFile = SymbolUtil.GetFileParams(img, ref baseAddr, ref fileSize);
			NativeHelp.Call(getFile);

			_modBase = DbgHelp.SymLoadModule64(
				_proc,          // Process handle of the current process
				IntPtr.Zero,    // Handle to the module's image file (not needed)
				img,            // Path/name of the file
				null,           // User-defined short name of the module (it can be NULL)
				baseAddr,       // Base address of the module (cannot be NULL if .PDB file is used, otherwise it can be NULL)
				(uint) fileSize // Size of the file (cannot be NULL if .PDB file is used, otherwise it can be NULL)
			);

			CheckModule();
		}

		internal static long[] GetSymOffsets(string[] names)
		{
			return GetSymbols(names).Select(x => x.Offset).ToArray();
		}

		internal static long GetSymOffset(string name)
		{
			return GetSymbol(name).Offset;
		}

		internal static Symbol[] GetSymbols()
		{
			CheckModule();

			_symBuffer = new List<Symbol>();

			bool symEnumSuccess = DbgHelp.SymEnumSymbols(
				_proc,              // Process handle of the current process
				_modBase,           // Base address of the module
				null,               // Mask (NULL -> all symbols)
				CollectSymCallback, // The callback function
				IntPtr.Zero         // A used-defined context can be passed here, if necessary
			);

			NativeHelp.Call(symEnumSuccess, nameof(DbgHelp.SymEnumSymbols));


			Symbol[] cpy = _symBuffer.ToArray();
			ClearBuffer();

			return cpy;
		}

		internal static Symbol[] GetSymbols(string[] names)
		{
			CheckModule();

			var rg = new Symbol[names.Length];

			for (int i = 0; i < rg.Length; i++) {
				rg[i] = GetSymbol(names[i]);
			}

			return rg;
		}

		// note: doesn't check module
		internal static Symbol GetSymbol(string name)
		{
			//CheckModule();

			int sz = (int) (SymbolInfo.SIZE + DbgHelp.MAX_SYM_NAME * sizeof(byte) + sizeof(ulong) - 1 / sizeof(ulong));

			Pointer<SymbolInfo> buffer = stackalloc byte[sz];
			buffer.Reference.SizeOfStruct = (uint) SymbolInfo.SIZE;
			buffer.Reference.MaxNameLen   = DbgHelp.MAX_SYM_NAME;


			if (DbgHelp.SymFromName(_proc, name, buffer.Address)) {
				fixed (sbyte* firstChar = &buffer.Reference.Name) {
					var symName = NativeHelp.GetStringAlt(firstChar, (int) buffer.Reference.NameLen);
					return new Symbol(buffer.ToPointer<SymbolInfo>(), symName);
				}
			}

			throw new Exception(String.Format("Symbol \"{0}\" not found", name));
		}

		private static void ClearBuffer()
		{
			_symBuffer?.Clear();

			_singleNameBuffer = null;
			_symBuffer        = null;
//			_nameBuffer       = null;
		}

		internal static Symbol[] GetSymbolsContainingName(string name)
		{
			CheckModule();

			_symBuffer        = new List<Symbol>();
			_singleNameBuffer = name;

			bool symEnumSuccess = DbgHelp.SymEnumSymbols(
				_proc,                   // Process handle of the current process
				_modBase,                // Base address of the module
				null,                    // Mask (NULL -> all symbols)
				SymNameContainsCallback, // The callback function
				IntPtr.Zero              // A used-defined context can be passed here, if necessary
			);

			NativeHelp.Call(symEnumSuccess, nameof(DbgHelp.SymEnumSymbols));


			Symbol[] cpy = _symBuffer.ToArray();
			
			ClearBuffer();

			return cpy;
		}

		#region Callbacks

		private static bool SymNameContainsCallback(IntPtr sym, uint symSize, IntPtr userCtx)
		{
			var    pSym    = (SymbolInfo*) sym;
			string symName = NativeHelp.GetString(&pSym->Name, pSym->NameLen);

			if (symName.Contains(_singleNameBuffer)) {
				_symBuffer.Add(new Symbol(pSym));
			}

			return true;
		}

		private static bool CollectSymCallback(IntPtr sym, uint symSize, IntPtr userCtx)
		{
			var pSym = (SymbolInfo*) sym;

			_symBuffer.Add(new Symbol(pSym));

			return true;
		}

		#endregion
	}
}