#region

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using RazorCommon;
using RazorSharp.Memory;
using RazorSharp.Native.Structures;
using RazorSharp.Native.Structures.Images;
using RazorSharp.Pointers;

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

		internal const int TI_GET_CHILDRENCOUNT = 13;

		[return: MarshalAs(UnmanagedType.Bool)]
		internal delegate bool native_SYM_ENUMERATESYMBOLS_CALLBACK( /*SYMBOL_INFO*/ IntPtr symInfo,
		                                                                             uint   symbolSize,
		                                                                             IntPtr pUserContext);

		public enum IMAGEHLP_SYMBOL_TYPE_INFO : uint
		{
			/// <summary>
			///    The symbol tag. The data type is DWORD.
			/// </summary>
			TI_GET_SYMTAG,

			/// <summary>
			///    The symbol name. The data type is WCHAR*. The caller must free the buffer.
			/// </summary>
			TI_GET_SYMNAME,

			/// <summary>
			///    The length of the type. The data type is ULONG64.
			/// </summary>
			TI_GET_LENGTH,

			/// <summary>
			///    The type. The data type is DWORD.
			/// </summary>
			TI_GET_TYPE,

			/// <summary>
			///    The type index. The data type is DWORD.
			/// </summary>
			TI_GET_TYPEID,

			/// <summary>
			///    The base type for the type index. The data type is DWORD.
			/// </summary>
			TI_GET_BASETYPE,

			/// <summary>
			///    The type index for index of an array type. The data type is DWORD.
			/// </summary>
			TI_GET_ARRAYINDEXTYPEID,

			/// <summary>
			///    The type index of all children. The data type is a pointer to a
			///    TI_FINDCHILDREN_PARAMS structure. The Count member should be initialized
			///    with the number of children.
			/// </summary>
			TI_FINDCHILDREN,

			/// <summary>
			///    The data kind. The data type is DWORD.
			/// </summary>
			TI_GET_DATAKIND,

			/// <summary>
			///    The address offset. The data type is DWORD.
			/// </summary>
			TI_GET_ADDRESSOFFSET,

			/// <summary>
			///    The offset of the type in the parent. Members can use this to get their
			///    offset in a structure. The data type is DWORD.
			/// </summary>
			TI_GET_OFFSET,

			/// <summary>
			///    The value of a constant or enumeration value. The data type is VARIANT.
			/// </summary>
			TI_GET_VALUE,

			/// <summary>
			///    The count of array elements. The data type is DWORD.
			/// </summary>
			TI_GET_COUNT,

			/// <summary>
			///    The number of children. The data type is DWORD.
			/// </summary>
			TI_GET_CHILDRENCOUNT,

			/// <summary>
			///    The bit position of a bitfield. The data type is DWORD.
			/// </summary>
			TI_GET_BITPOSITION,

			/// <summary>
			///    A value that indicates whether the base class is virtually inherited. The
			///    data type is BOOL.
			/// </summary>
			TI_GET_VIRTUALBASECLASS,

			/// <summary>
			///    The symbol interface of the type of virtual table, for a user-defined type. The data type is DWORD.
			/// </summary>
			TI_GET_VIRTUALTABLESHAPEID,

			/// <summary>
			///    The offset of the virtual base pointer. The data type is DWORD.
			/// </summary>
			TI_GET_VIRTUALBASEPOINTEROFFSET,

			/// <summary>
			///    The type index of the class parent. The data type is DWORD.
			/// </summary>
			TI_GET_CLASSPARENTID,

			/// <summary>
			///    A value that indicates whether the type index is nested. The data type is
			///    DWORD.
			/// </summary>
			TI_GET_NESTED,

			/// <summary>
			///    The symbol index for a type. The data type is DWORD.
			/// </summary>
			TI_GET_SYMINDEX,

			/// <summary>
			///    The lexical parent of the type. The data type is DWORD.
			/// </summary>
			TI_GET_LEXICALPARENT,

			/// <summary>
			///    The index address. The data type is ULONG64.
			/// </summary>
			TI_GET_ADDRESS,

			/// <summary>
			///    The offset from the this pointer to its actual value. The data type is
			///    DWORD.
			/// </summary>
			TI_GET_THISADJUST,

			/// <summary>
			///    The UDT kind. The data type is DWORD.
			/// </summary>
			TI_GET_UDTKIND,

			/// <summary>
			///    The equivalency of two types. The data type is DWORD. The value is S_OK is
			///    the two types are equivalent, and S_FALSE otherwise.
			/// </summary>
			TI_IS_EQUIV_TO,

			/// <summary>
			///    The calling convention. The data type is DWORD.
			/// </summary>
			TI_GET_CALLING_CONVENTION,

			/// <summary>
			///    The equivalency of two symbols. This is not guaranteed to be accurate. The
			///    data type is DWORD. The value is S_OK is the two types are equivalent, and
			///    S_FALSE otherwise.
			/// </summary>
			TI_IS_CLOSE_EQUIV_TO,

			/// <summary>
			///    The element where the valid request bitfield should be stored. The data
			///    type is ULONG64.
			///
			///    This value is only used with the SymGetTypeInfoEx function.
			/// </summary>
			TI_GTIEX_REQS_VALID,

			/// <summary>
			///    The offset in the virtual function table of a virtual function. The data
			///    type is DWORD.
			/// </summary>
			TI_GET_VIRTUALBASEOFFSET,

			/// <summary>
			///    The index into the virtual base displacement table. The data type is DWORD.
			/// </summary>
			TI_GET_VIRTUALBASEDISPINDEX,

			/// <summary>
			///    Indicates whether a pointer type is a reference. The data type is Boolean.
			/// </summary>
			TI_GET_IS_REFERENCE,

			/// <summary>
			///    Indicates whether the user-defined data type is an indirect virtual base.
			///    The data type is BOOL.
			/// </summary>
			TI_GET_INDIRECTVIRTUALBASECLASS,
			IMAGEHLP_SYMBOL_TYPE_INFO_MAX,
		} // end enum IMAGEHLP_SYMBOL_TYPE_INFO

		//BOOL IMAGEAPI SymGetTypeInfo(
		//	HANDLE                    hProcess,
		//	DWORD64                   ModBase,
		//	ULONG                     TypeId,
		//	IMAGEHLP_SYMBOL_TYPE_INFO GetType,
		//	PVOID                     pInfo
		//);
		[DefaultDllImportSearchPaths(DllImportSearchPath.LegacyBehavior)]
		[DllImport(DBG_HELP_DLL, SetLastError = true, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool SymGetTypeInfo(IntPtr                    hProcess, ulong modBase, uint typeId,
		                                           IMAGEHLP_SYMBOL_TYPE_INFO getType,  void* pInfo);

		[DefaultDllImportSearchPaths(DllImportSearchPath.LegacyBehavior)]
		[DllImport(DBG_HELP_DLL,
			SetLastError = true,
			CharSet      = CharSet.Unicode,
			EntryPoint   = "SymEnumTypesByNameW")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool SymEnumTypesByName(IntPtr                               hProcess,
		                                               ulong                                modBase,
		                                               string                               mask,
		                                               native_SYM_ENUMERATESYMBOLS_CALLBACK callback,
		                                               IntPtr                               pUserContext);

		[DefaultDllImportSearchPaths(DllImportSearchPath.LegacyBehavior)]
		[DllImport(DBG_HELP_DLL,
			SetLastError = true,
			CharSet      = CharSet.Unicode,
			EntryPoint   = "SymEnumSymbolsW")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool SymEnumSymbols(IntPtr                                      hProcess,
		                                           ulong                                       modBase,
		                                           [MarshalAs(UnmanagedType.LPUTF8Str)] string mask,
		                                           native_SYM_ENUMERATESYMBOLS_CALLBACK        callback,
		                                           IntPtr                                      pUserContext);

		[DefaultDllImportSearchPaths(DllImportSearchPath.LegacyBehavior)]
		[DllImport(DBG_HELP_DLL,
			SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool SymEnumSymbols(IntPtr                               hProcess,
		                                           ulong                                modBase,
		                                           IntPtr                               mask,
		                                           native_SYM_ENUMERATESYMBOLS_CALLBACK callback,
		                                           IntPtr                               pUserContext);

		[DefaultDllImportSearchPaths(DllImportSearchPath.LegacyBehavior)]
		[DllImport(DBG_HELP_DLL,
			SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool SymEnumSymbols(IntPtr hProcess,
		                                           ulong  modBase,
		                                           IntPtr mask,
		                                           IntPtr callback,
		                                           IntPtr pUserContext);

		[DllImport(DBG_HELP_DLL)]
		private static extern ImageNtHeaders64* ImageNtHeader(IntPtr hModule);

		[Flags]
		public enum SymEnumOptions
		{
			Default = 0x01,
			Inline  = 0x02, // includes inline symbols

			All = (Default | Inline)
		}

		[Flags]
		public enum SymSearchOptions
		{
			MaskObjs    = 0x01, // used internally to implement other APIs
			Recurse     = 0x02, // recurse scopes
			GlobalsOnly = 0x04, // search only for global symbols
			AllItems    = 0x08, // search for everything in the pdb, not just normal scoped symbols
		}

		internal enum ImageDirectoryEntry : ushort
		{
			EXPORT    = 0, // Export Directory
			IMPORT    = 1, // Import Directory
			RESOURCE  = 2, // Resource Directory
			EXCEPTION = 3, // Exception Directory
			SECURITY  = 4, // Security Directory
			BASERELOC = 5, // Base Relocation Table
			DEBUG     = 6, // Debug Directory

			// COPYRIGHT      =  7,  // (X86 usage)
			ARCHITECTURE   = 7,  // Architecture Specific Data
			GLOBALPTR      = 8,  // RVA of GP
			TLS            = 9,  // TLS Directory
			LOAD_CONFIG    = 10, // Load Configuration Directory
			BOUND_IMPORT   = 11, // Bound Import Directory in headers
			IAT            = 12, // Import Address Table
			DELAY_IMPORT   = 13, // Delay Load Import Descriptors
			COM_DESCRIPTOR = 14  // COM Runtime descriptor
		}                        // end enum ImageDirectoryEntry

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


		// BOOL SymInitialize(HANDLE hProcess, PCSTR UserSearchPath, BOOL fInvadeProcess)
		[DefaultDllImportSearchPaths(DllImportSearchPath.LegacyBehavior)]
		[DllImport(DBG_HELP_DLL,
			SetLastError = true,
			CharSet      = CharSet.Unicode,
			EntryPoint   = "SymInitialize")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SymInitialize(IntPtr                               hProcess, void* userSearchPath,
		                                         [MarshalAs(UnmanagedType.Bool)] bool fInvadeProcess);

		[DefaultDllImportSearchPaths(DllImportSearchPath.LegacyBehavior)]
		[DllImport("dbghelp.dll",
			SetLastError = true,
			CharSet      = CharSet.Unicode,
			EntryPoint   = "SymLoadModuleExW")]
		internal static extern ulong SymLoadModuleEx(IntPtr                  hProcess,
		                                             IntPtr                  hFile,
		                                             string                  ImageName,
		                                             string                  ModuleName,
		                                             ulong                   BaseOfDll,
		                                             uint                    DllSize,
		                                             /*MODLOAD_DATA*/ IntPtr Data,
		                                             uint                    Flags);

		[DefaultDllImportSearchPaths(DllImportSearchPath.LegacyBehavior)]
		[DllImport("dbghelp.dll",
			SetLastError = true)]
		internal static extern ulong SymLoadModuleEx(IntPtr                  hProcess,
		                                             IntPtr                  hFile,
		                                             IntPtr                  ImageName,
		                                             IntPtr                  ModuleName,
		                                             ulong                   BaseOfDll,
		                                             uint                    DllSize,
		                                             /*MODLOAD_DATA*/ IntPtr Data,
		                                             uint                    Flags);

		[DefaultDllImportSearchPaths(DllImportSearchPath.LegacyBehavior)]
		[DllImport("dbghelp.dll",
			SetLastError = true,
			CharSet      = CharSet.Unicode)]
		internal static extern ulong SymLoadModuleEx(IntPtr                  hProcess,
		                                             IntPtr                  hFile,
		                                             StringBuilder           ImageName,
		                                             StringBuilder           ModuleName,
		                                             ulong                   BaseOfDll,
		                                             uint                    DllSize,
		                                             /*MODLOAD_DATA*/ IntPtr Data,
		                                             uint                    Flags);


		// BOOL SymEnumTypes(HANDLE hProcess, ULONG64 BaseOfDll, PSYM_ENUMERATESYMBOLS_CALLBACK EnumSymbolsCallback, PVOID UserContext)

		[DefaultDllImportSearchPaths(DllImportSearchPath.LegacyBehavior)]
		[DllImport(DBG_HELP_DLL,
			SetLastError = true,
			CharSet      = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool SymEnumTypes(IntPtr                               hProcess,
		                                         ulong                                modBase,
		                                         native_SYM_ENUMERATESYMBOLS_CALLBACK callback,
		                                         IntPtr                               pUserContext);

		// BOOL SymCleanup(HANDLE hProcess)
		[DefaultDllImportSearchPaths(DllImportSearchPath.LegacyBehavior)]
		[DllImport(DBG_HELP_DLL, SetLastError = true, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool SymCleanup(IntPtr hProcess);

		private static IntPtr _hProcess;


		internal static bool SymGet(string image, string userContext, out long offset)
		{
			_hProcess = Kernel32.GetCurrentProcess();

			var ctxStrNative = Mem.AllocString(userContext).Address;
			var imgStrNative = Mem.AllocString(image).Address;
			var maskStr = Mem.AllocString("*!*").Address;
			
			Debug.Assert(SymInitialize(_hProcess, null, false));
			const ulong BASE = 0x400000;
			const uint  SIZE = 0x20000;

			ulong dllBase = SymLoadModuleEx(_hProcess,
			                                IntPtr.Zero,
			                                imgStrNative,
			                                IntPtr.Zero,
			                                (ulong) BASE /*0x400000*/,
			                                (uint) SIZE /*0x20000*/,
			                                IntPtr.Zero,
			                                0);


			

			Debug.Assert(SymEnumSymbols(_hProcess, dllBase, maskStr, EnumSymProc, ctxStrNative));
			Debug.Assert(SymEnumTypes(_hProcess, dllBase, EnumSymProc, ctxStrNative));
			Debug.Assert(SymCleanup(_hProcess));

			offset = (_addr - (int) BASE).ToInt64();


			Mem.FreeString(ctxStrNative);
			Mem.FreeString(imgStrNative);
			Mem.FreeString(maskStr);

			_hProcess = IntPtr.Zero;
			_addr     = IntPtr.Zero;


			return true;
		}

		internal static Pointer<byte> GetFuncAddr(string image, string module, string name)
		{
			Debug.Assert(SymGet(image, name, out var offset));
			var           pm  = Modules.GetModule(module);
			Pointer<byte> ptr = pm.BaseAddress;
			return ptr + offset;
		}

		private static IntPtr _addr;

		internal static bool EnumSymProc(IntPtr pSymInfoX, uint ul, IntPtr userContext)
		{
			Debug.Assert(pSymInfoX != IntPtr.Zero);
			SymbolInfo* pSymInfo  = (SymbolInfo*) pSymInfoX;
			var         strR      = Marshal.PtrToStringAnsi(userContext);
			int         maxcmplen = strR.Length;

			if (maxcmplen == pSymInfo->NameLen) {
				var s = Marshal.PtrToStringAnsi(new IntPtr(&pSymInfo->Name), (int) pSymInfo->NameLen);

				if ((String.Compare(s, strR)) == 0) {
					TI_FINDCHILDREN_PARAMS childs = new TI_FINDCHILDREN_PARAMS();
					SymGetTypeInfo(_hProcess, pSymInfo->ModBase, pSymInfo->TypeIndex,
					               IMAGEHLP_SYMBOL_TYPE_INFO.TI_GET_CHILDRENCOUNT,
					               &childs.Count);

					_addr = (IntPtr) pSymInfo->Address;
					//Console.WriteLine("Size: {0} | Type index: {1} | Children: {2} | Address: {3} | Name: {4}",
					//                  pSymInfo->Size, pSymInfo->TypeIndex, childs.Count, pSymInfo->Address,s);
				}
			}

			return true;
		}

		public static ImageSectionInfo[] GetPESectionInfo(IntPtr hModule)
		{
			// get the location of the module's IMAGE_NT_HEADERS structure
			ImageNtHeaders64* pNtHdr = ImageNtHeader(hModule);

			// section table immediately follows the IMAGE_NT_HEADERS
			var pSectionHdr = (IntPtr) (pNtHdr + 1);
			var imageBase   = hModule;

			var arr = new ImageSectionInfo[pNtHdr->FileHeader.NumberOfSections];

			for (int scn = 0; scn < pNtHdr->FileHeader.NumberOfSections; ++scn) {
				var struc = Marshal.PtrToStructure<ImageSectionHeader>(pSectionHdr);

				arr[scn] = new ImageSectionInfo(scn, struc.Name, (void*) (imageBase.ToInt64() + struc.VirtualAddress),
				                                (int) struc.VirtualSize, struc);

				pSectionHdr += Marshal.SizeOf<ImageSectionHeader>();
			}

			return arr;
		}
	}
}