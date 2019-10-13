using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using RazorSharp.Core;
using RazorSharp.Model;
using RazorSharp.Native.Enums;
using RazorSharp.Native.Structures;
using RazorSharp.Native.Win32;
using RazorSharp.Utilities.Security;
using SimpleSharp.Diagnostics;

// ReSharper disable ReturnTypeCanBeEnumerable.Global
// ReSharper disable RedundantAssignment

namespace RazorSharp.Import
{
	/// <summary>
	/// Provides access to symbols in a specified image
	/// <para>https://github.com/Microsoft/microsoft-pdb</para>
	/// </summary>
	internal sealed class SymbolManager : Releasable
	{
		protected override string Id => nameof(SymbolManager);

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

		private SymbolManager()
		{
			Setup();
		}

		/// <summary>
		/// Gets an instance of <see cref="SymbolManager"/>
		/// </summary>
		internal static SymbolManager Value { get; private set; } = new SymbolManager();

		#endregion

		public override void Close()
		{
			UnloadModule();
			NativeWin32.Debug.SymCleanup(m_proc);

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
			m_proc = NativeWin32.Kernel.GetCurrentProcess();

			var options = NativeWin32.Debug.SymGetOptions();

			// SYMOPT_DEBUG option asks DbgHelp to print additional troubleshooting
			// messages to debug output - use the debugger's Debug Output window
			// to view the messages

			options |= SymbolOptions.DEBUG;

			NativeWin32.Debug.SymSetOptions(options);

			// Initialize DbgHelp and load symbols for all modules of the current process 
			NativeWin32.Debug.SymInitialize(m_proc);


			base.Setup();
		}

		private void UnloadModule()
		{
			if (IsImageLoaded) {
				NativeWin32.Debug.SymUnloadModule64(m_proc, m_modBase);
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

			Global.Value.WriteVerbose(Id, "Loading image {Img}", m_pdb.Name);

			UnloadModule();
			
			Conditions.Require(IsSetup, nameof(IsSetup));

			NativeWin32.Kernel.GetFileParams(img, out ulong baseAddr, out ulong fileSize);


			m_modBase = NativeWin32.Debug.SymLoadModuleEx(
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

			NativeWin32.Debug.SymEnumSymbols(
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

			return NativeWin32.Debug.GetSymbol(m_proc, name);
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

			NativeWin32.Debug.SymEnumSymbols(
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
			string symName = NativeWin32.Debug.GetSymbolName(sym);

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