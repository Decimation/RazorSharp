#region

using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using RazorSharp.Native.Images;
using RazorSharp.Native.Symbols;

#endregion

// ReSharper disable InconsistentNaming

namespace RazorSharp.Native.Win32
{
	/// <summary>
	///     https://github.com/Microsoft/DbgShell/blob/master/DbgProvider/internal/Native/DbgHelp.cs
	/// </summary>
	internal static unsafe class DbgHelp
	{
		private const string DBG_HELP_DLL = "DbgHelp.dll";

		internal const uint SYMOPT_DEBUG = 0x80000000;

		internal const uint SYMOPT_UNDNAME = 0x2;

		internal const uint MAX_SYM_NAME = 2000;


		[DllImport(DBG_HELP_DLL)]
		internal static extern ImageNtHeaders64* ImageNtHeader(IntPtr hModule);

		#region Sym misc

		[SuppressUnmanagedCodeSecurity]
		[DefaultDllImportSearchPaths(DllImportSearchPath.LegacyBehavior)]
		[DllImport(DBG_HELP_DLL, SetLastError = true, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool SymInitialize(IntPtr                               hProcess,
		                                          void*                                userSearchPath,
		                                          [MarshalAs(UnmanagedType.Bool)] bool fInvadeProcess);


		[SuppressUnmanagedCodeSecurity]
		[DefaultDllImportSearchPaths(DllImportSearchPath.LegacyBehavior)]
		[DllImport(DBG_HELP_DLL, SetLastError = true, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool SymGetTypeInfo(IntPtr                  hProcess, ulong modBase, uint typeId,
		                                           ImageHelpSymbolTypeInfo getType,  void* pInfo);

		[SuppressUnmanagedCodeSecurity]
		[DefaultDllImportSearchPaths(DllImportSearchPath.LegacyBehavior)]
		[DllImport(DBG_HELP_DLL, SetLastError = true, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool SymCleanup(IntPtr hProcess);

		/// <summary>
		///     SYM_ENUMERATESYMBOLS_CALLBACK
		/// </summary>
		/// <param name="symInfo">SYMBOL_INFO</param>
		[return: MarshalAs(UnmanagedType.Bool)]
		internal delegate bool SymEnumSymbolsCallback(IntPtr symInfo, uint symbolSize, IntPtr pUserContext);

		#endregion

		#region Sym enum types

		[DefaultDllImportSearchPaths(DllImportSearchPath.LegacyBehavior)]
		[DllImport(DBG_HELP_DLL, SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "SymEnumTypesByNameW")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool SymEnumTypesByName(IntPtr                 hProcess,
		                                               ulong                  modBase,
		                                               string                 mask,
		                                               SymEnumSymbolsCallback callback,
		                                               IntPtr                 pUserContext);

		[SuppressUnmanagedCodeSecurity]
		[DefaultDllImportSearchPaths(DllImportSearchPath.LegacyBehavior)]
		[DllImport(DBG_HELP_DLL, SetLastError = true, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool SymEnumTypes(IntPtr                 hProcess,
		                                         ulong                  modBase,
		                                         SymEnumSymbolsCallback callback,
		                                         IntPtr                 pUserContext);

		#endregion

		#region Sym enum symbols

		[DefaultDllImportSearchPaths(DllImportSearchPath.LegacyBehavior)]
		[DllImport(DBG_HELP_DLL, SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "SymEnumSymbolsW")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool SymEnumSymbols(IntPtr                 hProcess,
		                                           ulong                  modBase,
		                                           string                 mask,
		                                           SymEnumSymbolsCallback callback,
		                                           IntPtr                 pUserContext);


		[SuppressUnmanagedCodeSecurity]
		[DefaultDllImportSearchPaths(DllImportSearchPath.LegacyBehavior)]
		[DllImport(DBG_HELP_DLL, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool SymEnumSymbols(IntPtr                 hProcess,
		                                           ulong                  modBase,
		                                           IntPtr                 mask,
		                                           SymEnumSymbolsCallback callback,
		                                           IntPtr                 pUserContext);

		[DefaultDllImportSearchPaths(DllImportSearchPath.LegacyBehavior)]
		[DllImport(DBG_HELP_DLL, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool SymEnumSymbols(IntPtr hProcess,
		                                           ulong  modBase,
		                                           IntPtr mask,
		                                           IntPtr callback,
		                                           IntPtr pUserContext);

		#endregion

		#region Sym options

		[DllImport(DBG_HELP_DLL)]
		internal static extern uint SymGetOptions();

		[DllImport(DBG_HELP_DLL)]
		internal static extern uint SymSetOptions(uint options);

		#endregion

		#region Sym from

		[DllImport(DBG_HELP_DLL)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool SymFromName(IntPtr hProcess, IntPtr name, IntPtr pSymbol);

		[DllImport(DBG_HELP_DLL)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool SymFromName(IntPtr hProcess, string name, IntPtr pSymbol);
		
		
		[DllImport(DBG_HELP_DLL, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool SymFromAddr(IntPtr hProc, ulong addr, ulong* displacement, SymbolInfo* pSym);

		#endregion

		#region Sym module

		[DllImport(DBG_HELP_DLL)]
		internal static extern uint SymLoadModule64(IntPtr hProc, IntPtr h, string p, string s, ulong baseAddr,
		                                            uint   fileSize);
		
		[DllImport(DBG_HELP_DLL)]
		internal static extern uint SymLoadModuleEx(IntPtr hProc, IntPtr h, string p, string s, ulong baseAddr,
		                                            uint   fileSize);

		// BOOL SymGetModuleInfo64(HANDLE hProcess, DWORD64 qwAddr, PIMAGEHLP_MODULE64 ModuleInfo)
		[DllImport(DBG_HELP_DLL, SetLastError = true)]
		internal static extern bool SymGetModuleInfo64(IntPtr hProc, ulong qwAddr, IntPtr pModInfo);

		// BOOL SymUnloadModule64(HANDLE hProcess, DWORD64 BaseOfDll)
		[DllImport(DBG_HELP_DLL)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool SymUnloadModule64(IntPtr hProc, ulong baseAddr);

		[DefaultDllImportSearchPaths(DllImportSearchPath.LegacyBehavior)]
		[DllImport(DBG_HELP_DLL, SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "SymLoadModuleExW")]
		internal static extern ulong SymLoadModuleEx(IntPtr hProcess,
		                                             IntPtr hFile,
		                                             string imageName,
		                                             string moduleName,
		                                             ulong  baseOfDll,
		                                             uint   dllSize,
		                                             IntPtr data,
		                                             uint   flags);


		[SuppressUnmanagedCodeSecurity]
		[DefaultDllImportSearchPaths(DllImportSearchPath.LegacyBehavior)]
		[DllImport(DBG_HELP_DLL, SetLastError = true)]
		internal static extern ulong SymLoadModuleEx(IntPtr hProcess,
		                                             IntPtr hFile,
		                                             IntPtr imageName,
		                                             IntPtr moduleName,
		                                             ulong  baseOfDll,
		                                             uint   dllSize,
		                                             IntPtr data,
		                                             uint   flags);


		[DefaultDllImportSearchPaths(DllImportSearchPath.LegacyBehavior)]
		[DllImport(DBG_HELP_DLL, SetLastError = true, CharSet = CharSet.Unicode)]
		internal static extern ulong SymLoadModuleEx(IntPtr        hProcess,
		                                             IntPtr        hFile,
		                                             StringBuilder imageName,
		                                             StringBuilder moduleName,
		                                             ulong         baseOfDll,
		                                             uint          dllSize,
		                                             IntPtr        data,
		                                             uint          flags);

		#endregion
	}
}