#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using RazorCommon;
using RazorSharp.Memory;
using RazorSharp.Native.Enums;
using RazorSharp.Native.Structures;
using RazorSharp.Native.Structures.Images;
using RazorSharp.Pointers;
using RazorSharp.Utilities;

#endregion

// ReSharper disable InconsistentNaming

namespace RazorSharp.Native
{
	/// <summary>
	/// https://github.com/Microsoft/DbgShell/blob/master/DbgProvider/internal/Native/DbgHelp.cs
	/// </summary>
	public static unsafe class DbgHelp
	{
		private const string DBG_HELP_DLL = "DbgHelp.dll";

		/// <summary>
		/// SYM_ENUMERATESYMBOLS_CALLBACK
		/// </summary>
		/// <param name="symInfo">SYMBOL_INFO</param>
		/// <param name="symbolSize"></param>
		/// <param name="pUserContext"></param>
		[return: MarshalAs(UnmanagedType.Bool)]
		internal delegate bool SymEnumerateSymbols(IntPtr symInfo, uint symbolSize, IntPtr pUserContext);

		//BOOL IMAGEAPI SymGetTypeInfo(
		//	HANDLE                    hProcess,
		//	DWORD64                   ModBase,
		//	ULONG                     TypeId,
		//	IMAGEHLP_SYMBOL_TYPE_INFO GetType,
		//	PVOID                     pInfo
		//);
		[SuppressUnmanagedCodeSecurity]
		[DefaultDllImportSearchPaths(DllImportSearchPath.LegacyBehavior)]
		[DllImport(DBG_HELP_DLL, SetLastError = true, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool SymGetTypeInfo(IntPtr                  hProcess, ulong modBase, uint typeId,
		                                           ImageHelpSymbolTypeInfo getType,  void* pInfo);

		[DefaultDllImportSearchPaths(DllImportSearchPath.LegacyBehavior)]
		[DllImport(DBG_HELP_DLL, SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "SymEnumTypesByNameW")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool SymEnumTypesByName(IntPtr              hProcess,
		                                               ulong               modBase,
		                                               string              mask,
		                                               SymEnumerateSymbols callback,
		                                               IntPtr              pUserContext);

		[DefaultDllImportSearchPaths(DllImportSearchPath.LegacyBehavior)]
		[DllImport(DBG_HELP_DLL, SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "SymEnumSymbolsW")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool SymEnumSymbols(IntPtr              hProcess,
		                                           ulong               modBase,
		                                           string              mask,
		                                           SymEnumerateSymbols callback,
		                                           IntPtr              pUserContext);

		[SuppressUnmanagedCodeSecurity]
		[DefaultDllImportSearchPaths(DllImportSearchPath.LegacyBehavior)]
		[DllImport(DBG_HELP_DLL, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool SymEnumSymbols(IntPtr              hProcess,
		                                           ulong               modBase,
		                                           IntPtr              mask,
		                                           SymEnumerateSymbols callback,
		                                           IntPtr              pUserContext);

		[DefaultDllImportSearchPaths(DllImportSearchPath.LegacyBehavior)]
		[DllImport(DBG_HELP_DLL, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool SymEnumSymbols(IntPtr hProcess,
		                                           ulong  modBase,
		                                           IntPtr mask,
		                                           IntPtr callback,
		                                           IntPtr pUserContext);

		[DllImport(DBG_HELP_DLL)]
		private static extern ImageNtHeaders64* ImageNtHeader(IntPtr hModule);


		/*BOOL CALLBACK EnumSymProc(PSYMBOL_INFO pSymInfo, ULONG, PVOID UserContext)
		{
			size_t maxcmplen = strlen((PCHAR) UserContext);
			if (maxcmplen == pSymInfo->NameLen) {
				if ((strncmp(pSymInfo->Name, (PCHAR) UserContext, pSymInfo->NameLen)) == 0) {
					TI_FINDCHILDREN_PARAMS childs = {0};
					SymGetTypeInfo(hProcess, pSymInfo->ModBase, pSymInfo->TypeIndex,
					               TI_GET_CHILDRENCOUNT, &childs.Count);
					printf("%8s%10s%10s%16s %s", "Size", "TypeIndex", "Childs", "Address", "Name\n");
					printf("%8x %8x %8x %16I64x %10s\n", pSymInfo->Size, pSymInfo->TypeIndex,
					       childs.Count, pSymInfo->Address, pSymInfo->Name);
				}
			}
			return TRUE;
		}*/


		/// <summary>
		/// BOOL SymInitialize(HANDLE hProcess, PCSTR UserSearchPath, BOOL fInvadeProcess)
		/// </summary>
		/// <param name="hProcess"></param>
		/// <param name="userSearchPath"></param>
		/// <param name="fInvadeProcess"></param>
		/// <returns></returns>
		[SuppressUnmanagedCodeSecurity]
		[DefaultDllImportSearchPaths(DllImportSearchPath.LegacyBehavior)]
		[DllImport(DBG_HELP_DLL, SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "SymInitialize")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool SymInitialize(IntPtr                               hProcess,
		                                          void*                                userSearchPath,
		                                          [MarshalAs(UnmanagedType.Bool)] bool fInvadeProcess);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="hProcess"></param>
		/// <param name="hFile"></param>
		/// <param name="imageName"></param>
		/// <param name="moduleName"></param>
		/// <param name="baseOfDll"></param>
		/// <param name="dllSize"></param>
		/// <param name="data">MODLOAD_DATA</param>
		/// <param name="flags"></param>
		/// <returns></returns>
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

		/// <summary>
		/// 
		/// </summary>
		/// <param name="hProcess"></param>
		/// <param name="hFile"></param>
		/// <param name="imageName"></param>
		/// <param name="moduleName"></param>
		/// <param name="baseOfDll"></param>
		/// <param name="dllSize"></param>
		/// <param name="data">MODLOAD_DATA</param>
		/// <param name="flags"></param>
		/// <returns></returns>
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

		/// <summary>
		/// 
		/// </summary>
		/// <param name="hProcess"></param>
		/// <param name="hFile"></param>
		/// <param name="imageName"></param>
		/// <param name="moduleName"></param>
		/// <param name="baseOfDll"></param>
		/// <param name="dllSize"></param>
		/// <param name="data">MODLOAD_DATA</param>
		/// <param name="flags"></param>
		/// <returns></returns>
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


		/// <summary>
		/// BOOL SymEnumTypes(HANDLE hProcess, ULONG64 BaseOfDll, PSYM_ENUMERATESYMBOLS_CALLBACK EnumSymbolsCallback, PVOID UserContext)
		/// </summary>
		/// <param name="hProcess"></param>
		/// <param name="modBase"></param>
		/// <param name="callback"></param>
		/// <param name="pUserContext"></param>
		/// <returns></returns>
		[SuppressUnmanagedCodeSecurity]
		[DefaultDllImportSearchPaths(DllImportSearchPath.LegacyBehavior)]
		[DllImport(DBG_HELP_DLL, SetLastError = true, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool SymEnumTypes(IntPtr              hProcess,
		                                         ulong               modBase,
		                                         SymEnumerateSymbols callback,
		                                         IntPtr              pUserContext);

		/// <summary>
		/// BOOL SymCleanup(HANDLE hProcess)
		/// </summary>
		/// <param name="hProcess"></param>
		/// <returns></returns>
		[SuppressUnmanagedCodeSecurity]
		[DefaultDllImportSearchPaths(DllImportSearchPath.LegacyBehavior)]
		[DllImport(DBG_HELP_DLL, SetLastError = true, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool SymCleanup(IntPtr hProcess);


		public static ImageSectionInfo[] GetPESectionInfo(IntPtr hModule)
		{
			// get the location of the module's IMAGE_NT_HEADERS structure
			ImageNtHeaders64* pNtHdr = ImageNtHeader(hModule);

			// section table immediately follows the IMAGE_NT_HEADERS
			var pSectionHdr = (IntPtr) (pNtHdr + 1);
			var imageBase   = hModule;
			var arr         = new ImageSectionInfo[pNtHdr->FileHeader.NumberOfSections];
			int size        = Marshal.SizeOf<ImageSectionHeader>();

			for (int scn = 0; scn < pNtHdr->FileHeader.NumberOfSections; ++scn) {
				var struc = Marshal.PtrToStructure<ImageSectionHeader>(pSectionHdr);

				arr[scn] = new ImageSectionInfo(scn, struc.Name,
				                                (void*) (imageBase.ToInt64() + struc.VirtualAddress),
				                                (int) struc.VirtualSize, struc);

				pSectionHdr += size;
			}

			return arr;
		}
	}
}