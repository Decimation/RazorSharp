using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using NativeSharp.DebugHelp;
using NativeSharp.DebugHelp.Enums;
using NativeSharp.DebugHelp.Wrappers;
using NativeSharp.Kernel;
using RazorSharp.Model;
using RazorSharp.Utilities.Security;
using SimpleSharp.Diagnostics;

// ReSharper disable ReturnTypeCanBeEnumerable.Global
// ReSharper disable RedundantAssignment

namespace RazorSharp.Import
{
	/// <summary>
	/// Provides access to symbols in a specified image
	/// </summary>
	internal sealed class SymbolManager : Releasable
	{
		private const string CONTEXT = nameof(SymbolManager);

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
			DbgHelp.SymCleanup(m_proc);

			m_proc    = IntPtr.Zero;
			m_modBase = default;
			m_pdb     = null;

			ClearBuffer();

			// Delete instance
			Value = null;

			base.Close();
		}

		public override void Setup()
		{
			m_proc = Kernel32.GetCurrentProcess();

			var options = DbgHelp.SymGetOptions();

			// SYMOPT_DEBUG option asks DbgHelp to print additional troubleshooting
			// messages to debug output - use the debugger's Debug Output window
			// to view the messages

			options |= SymbolOptions.DEBUG;

			DbgHelp.SymSetOptions(options);

			// Initialize DbgHelp and load symbols for all modules of the current process 
			DbgHelp.SymInitialize(m_proc, IntPtr.Zero, false);


			base.Setup();
		}

		private void UnloadModule()
		{
			if (IsImageLoaded) {
				DbgHelp.SymUnloadModule64(m_proc, m_modBase);
			}
		}

		private void CheckModule()
		{
			if (!IsImageLoaded) {
				throw Guard.ImageFail("This may be an error with loading. Have you loaded an image?");
			}
		}

		private void Load()
		{
			string img = m_pdb.FullName;

			Global.Value.WriteVerbose(CONTEXT, "Loading image {Img}", m_pdb.Name);

			UnloadModule();
			Conditions.Require(IsSetup, nameof(IsSetup));

			Kernel32.GetFileParams(img, out ulong baseAddr, out ulong fileSize);


			m_modBase = DbgHelp.SymLoadModuleEx(
				m_proc,         // Process handle of the current process
				IntPtr.Zero,    // Handle to the module's image file (not needed)
				img,            // Path/name of the file
				null,           // User-defined short name of the module (it can be NULL)
				baseAddr,       // Base address of the module (cannot be NULL if .PDB file is used, otherwise it can be NULL)
				(uint) fileSize // Size of the file (cannot be NULL if .PDB file is used, otherwise it can be NULL)
//				IntPtr.Zero,
//				0x1
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

			DbgHelp.SymEnumSymbols(
				m_proc,             // Process handle of the current process
				m_modBase,          // Base address of the module
				null,               // Mask (NULL -> all symbols)
				CollectSymCallback, // The callback function
				IntPtr.Zero         // A used-defined context can be passed here, if necessary
			);

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

			return DbgHelp.GetSymbol(m_proc, name);
		}

		private void ClearBuffer()
		{
			m_symBuffer?.Clear();

			m_singleNameBuffer = null;
			m_symBuffer        = null;
		}

		internal Symbol[] GetSymbolsContainingName(string name)
		{
			CheckModule();

			m_symBuffer        = new List<Symbol>();
			m_singleNameBuffer = name;

			DbgHelp.SymEnumSymbols(
				m_proc,                  // Process handle of the current process
				m_modBase,               // Base address of the module
				null,                    // Mask (NULL -> all symbols)
				SymNameContainsCallback, // The callback function
				IntPtr.Zero              // A used-defined context can be passed here, if necessary
			);

			Symbol[] cpy = m_symBuffer.ToArray();

			ClearBuffer();

			return cpy;
		}

		#region Callbacks

		private bool SymNameContainsCallback(IntPtr sym, uint symSize, IntPtr userCtx)
		{
			string symName = DbgHelp.GetSymbolName(sym);

			if (symName.Contains(m_singleNameBuffer)) {
				m_symBuffer.Add(new Symbol(sym));
			}

			return true;
		}

		private bool CollectSymCallback(IntPtr sym, uint symSize, IntPtr userCtx)
		{
			m_symBuffer.Add(new Symbol(sym));

			return true;
		}

		#endregion
	}
}