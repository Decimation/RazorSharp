using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using RazorSharp.Components;
using RazorSharp.Memory.Pointers;
using RazorSharp.Native.Win32;
using SimpleSharp.Diagnostics;

// ReSharper disable ReturnTypeCanBeEnumerable.Global

namespace RazorSharp.Native.Symbols
{
	/// <summary>
	/// Provides access to symbols in a specified image
	/// </summary>
	internal sealed unsafe class SymbolManager : Releasable
	{
		private IntPtr       m_proc;
		private ulong        m_modBase;
		private string       m_singleNameBuffer;
		private List<Symbol> m_symBuffer;
		private FileInfo     m_pdb;

		internal bool IsImageLoaded {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => m_modBase != default;
		}

		internal FileInfo CurrentImage {
			get => m_pdb;
			set {
				if (m_pdb != value) {
					m_pdb = value;
					Load();
				}
			}
		}

		#region Singleton

		private SymbolManager() { }

		/// <summary>
		/// Gets an instance of <see cref="SymbolManager"/>
		/// </summary>
		public static SymbolManager Value { get; private set; } = new SymbolManager();

		#endregion

		public override void Close()
		{
			UnloadModule();
			NativeHelp.Call(DbgHelp.SymCleanup(m_proc), nameof(DbgHelp.SymCleanup));

			m_proc    = IntPtr.Zero;
			m_modBase = 0;
			m_pdb     = null;

			ClearBuffer();

			// Delete instance
			Value = null;

			base.Close();
		}

		public override void Setup()
		{
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

			base.Setup();
		}

		private void UnloadModule()
		{
			if (IsImageLoaded) {
				Global.Log.Verbose("Unloading image {Img}", m_pdb.Name);
				NativeHelp.Call(DbgHelp.SymUnloadModule64(m_proc, m_modBase), nameof(DbgHelp.SymUnloadModule64));
			}
		}

		private void CheckModule()
		{
			if (!IsImageLoaded) {
				string msg =
					$"Error loading image. This may be an error with {nameof(Load)}. Have you loaded an image?";

				throw new Exception(msg);
			}
		}

		private void Load()
		{
			string img = m_pdb.FullName;

			Global.Log.Verbose("Loading image {Img}", m_pdb.Name);

			UnloadModule();
			Conditions.Require(IsSetup, nameof(IsSetup));

			ulong baseAddr = 0;
			ulong fileSize = 0;

			bool getFile = SymbolUtil.GetFileParams(img, ref baseAddr, ref fileSize);
			NativeHelp.Call(getFile);

			m_modBase = DbgHelp.SymLoadModule64(
				m_proc,         // Process handle of the current process
				IntPtr.Zero,    // Handle to the module's image file (not needed)
				img,            // Path/name of the file
				null,           // User-defined short name of the module (it can be NULL)
				baseAddr,       // Base address of the module (cannot be NULL if .PDB file is used, otherwise it can be NULL)
				(uint) fileSize // Size of the file (cannot be NULL if .PDB file is used, otherwise it can be NULL)
			);

			CheckModule();
		}

		internal long[] GetSymOffsets(string[] names)
		{
			return GetSymbols(names).Select(x => x.Offset).ToArray();
		}

		internal long GetSymOffset(string name)
		{
			return GetSymbol(name).Offset;
		}

		internal Symbol[] GetSymbols()
		{
			CheckModule();

			m_symBuffer = new List<Symbol>();

			bool symEnumSuccess = DbgHelp.SymEnumSymbols(
				m_proc,             // Process handle of the current process
				m_modBase,          // Base address of the module
				null,               // Mask (NULL -> all symbols)
				CollectSymCallback, // The callback function
				IntPtr.Zero         // A used-defined context can be passed here, if necessary
			);

			NativeHelp.Call(symEnumSuccess, nameof(DbgHelp.SymEnumSymbols));


			Symbol[] cpy = m_symBuffer.ToArray();
			ClearBuffer();

			return cpy;
		}

		internal Symbol[] GetSymbols(string[] names)
		{
			CheckModule();

			var rg = new Symbol[names.Length];

			for (int i = 0; i < rg.Length; i++) {
				rg[i] = GetSymbol(names[i]);
			}

			return rg;
		}

		// note: doesn't check module
		internal Symbol GetSymbol(string name)
		{
			//CheckModule();

			int sz = (int) (SymbolInfo.SIZE + DbgHelp.MAX_SYM_NAME * sizeof(byte) + sizeof(ulong) - 1 / sizeof(ulong));

			Pointer<SymbolInfo> buffer = stackalloc byte[sz];
			buffer.Reference.SizeOfStruct = (uint) SymbolInfo.SIZE;
			buffer.Reference.MaxNameLen   = DbgHelp.MAX_SYM_NAME;

			if (DbgHelp.SymFromName(m_proc, name, buffer.Address)) {
				fixed (sbyte* firstChar = &buffer.Reference.Name) {
					var symName = NativeHelp.GetStringAlt(firstChar, (int) buffer.Reference.NameLen);
					return new Symbol(buffer.ToPointer<SymbolInfo>(), symName);
				}
			}

			throw new Exception(String.Format("Symbol \"{0}\" not found", name));
		}

		private void ClearBuffer()
		{
			m_symBuffer?.Clear();

			m_singleNameBuffer = null;
			m_symBuffer        = null;
//			_nameBuffer       = null;
		}

		internal Symbol[] GetSymbolsContainingName(string name)
		{
			CheckModule();

			m_symBuffer        = new List<Symbol>();
			m_singleNameBuffer = name;

			bool symEnumSuccess = DbgHelp.SymEnumSymbols(
				m_proc,                  // Process handle of the current process
				m_modBase,               // Base address of the module
				null,                    // Mask (NULL -> all symbols)
				SymNameContainsCallback, // The callback function
				IntPtr.Zero              // A used-defined context can be passed here, if necessary
			);

			NativeHelp.Call(symEnumSuccess, nameof(DbgHelp.SymEnumSymbols));


			Symbol[] cpy = m_symBuffer.ToArray();

			ClearBuffer();

			return cpy;
		}

		#region Callbacks

		private bool SymNameContainsCallback(IntPtr sym, uint symSize, IntPtr userCtx)
		{
			var    pSym    = (SymbolInfo*) sym;
			string symName = NativeHelp.GetString(&pSym->Name, pSym->NameLen);

			if (symName.Contains(m_singleNameBuffer)) {
				m_symBuffer.Add(new Symbol(pSym));
			}

			return true;
		}

		private bool CollectSymCallback(IntPtr sym, uint symSize, IntPtr userCtx)
		{
			var pSym = (SymbolInfo*) sym;

			m_symBuffer.Add(new Symbol(pSym));

			return true;
		}

		#endregion
	}
}