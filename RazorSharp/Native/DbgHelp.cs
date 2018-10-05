#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
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

	public static unsafe class DbgHelp
	{
		private const string DbgHelpDll = "DbgHelp.dll";

		[DllImport(DbgHelpDll, SetLastError = true, CharSet = CharSet.Ansi)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SymInitialize(IntPtr hProcess, string UserSearchPath,
			[MarshalAs(UnmanagedType.Bool)] bool InvadeProcess);

		//SymGetSymFromAddr64
		[DllImport(DbgHelpDll, SetLastError = true, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SymGetSymFromAddr64(IntPtr hProcess, ulong Address, [Out] ulong OffestFromSymbol, IntPtr Symbol);

		[DllImport(DbgHelpDll, SetLastError = true, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SymCleanup(IntPtr hProcess);

		/**
		 * BOOL IMAGEAPI StackWalk(
		   DWORD                          MachineType,
		   HANDLE                         hProcess,
		   HANDLE                         hThread,
		   LPSTACKFRAME                   StackFrame,
		   PVOID                          ContextRecord,
		   PREAD_PROCESS_MEMORY_ROUTINE   ReadMemoryRoutine,
		   PFUNCTION_TABLE_ACCESS_ROUTINE FunctionTableAccessRoutine,
		   PGET_MODULE_BASE_ROUTINE       GetModuleBaseRoutine,
		   PTRANSLATE_ADDRESS_ROUTINE     TranslateAddress
		 );
		 */
		//StackWalk64
		[DllImport(DbgHelpDll, SetLastError = true, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool StackWalk64
		(
			uint MachineType,                                            //In
			IntPtr hProcess,                                             //In
			IntPtr hThread,                                              //In
			IntPtr StackFrame,                                           //In_Out
			IntPtr ContextRecord,                                        //In_Out
			ReadProcessMemoryDelegate ReadMemoryRoutine,                 //_In_opt_
			SymFunctionTableAccess64Delegate FunctionTableAccessRoutine, //_In_opt_
			SymGetModuleBase64Delegate GetModuleBaseRoutine,             //_In_opt_
			TranslateAddressProc64Delegate TranslateAddress              //_In_opt_
		);


		[StructLayout(LayoutKind.Sequential)]
		public struct SYSTEM_INFO
		{
			public ushort ProcessorArchitecture;
			public ushort Reserved;
			public uint   PageSize;
			public IntPtr MinimumApplicationAddress;
			public IntPtr MaximumApplicationAddress;
			public IntPtr ActiveProcessorMask;
			public uint   NumberOfProcessors;
			public uint   ProcessorType;
			public uint   AllocationGranularity;
			public ushort ProcessorLevel;
			public ushort ProcessorRevision;
		}

		//StackWalk64 Callback Delegates
		public delegate bool ReadProcessMemoryDelegate(IntPtr hProcess, ulong lpBaseAddress, IntPtr lpBuffer,
			uint nSize, IntPtr lpNumberOfBytesRead);

		public delegate IntPtr SymFunctionTableAccess64Delegate(IntPtr hProcess, ulong AddrBase);

		public delegate ulong SymGetModuleBase64Delegate(IntPtr hProcess, ulong Address);

		public delegate ulong TranslateAddressProc64Delegate(IntPtr hProcess, IntPtr hThread, IntPtr lpAddress64);

		//SymLoadModuleEx
		[DllImport(DbgHelpDll, SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern ulong SymLoadModuleEx(IntPtr hProcess, IntPtr hFile, string ImageName, string ModuleName,
			IntPtr BaseOfDll, int DllSize, IntPtr Data, int Flags);

		//SymFunctionTableAccess64
		[DllImport(DbgHelpDll, SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern IntPtr SymFunctionTableAccess64(IntPtr hProcess, ulong AddrBase);

		//SymGetModuleBase64
		[DllImport(DbgHelpDll, SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern ulong SymGetModuleBase64(IntPtr hProcess, ulong dwAddr);


		[DllImport(DbgHelpDll)]
		private static extern ImageNtHeaders64* ImageNtHeader(IntPtr hModule);


		public static ImageSectionInfo[] GetPESectionInfo(IntPtr hModule)
		{
			// get the location of the module's IMAGE_NT_HEADERS structure
			ImageNtHeaders64* pNtHdr = ImageNtHeader(hModule);

			// section table immediately follows the IMAGE_NT_HEADERS
			IntPtr pSectionHdr = (IntPtr) (pNtHdr + 1);
			IntPtr imageBase   = hModule;

			ImageSectionInfo[] arr = new ImageSectionInfo[pNtHdr->FileHeader.NumberOfSections];

			for (int scn = 0; scn < pNtHdr->FileHeader.NumberOfSections; ++scn) {
				ImageSectionHeader struc = Marshal.PtrToStructure<ImageSectionHeader>(pSectionHdr);

				arr[scn] = new ImageSectionInfo(scn, struc.Name, (void*) (imageBase.ToInt64() + struc.VirtualAddress),
					(int) struc.VirtualSize, struc);

				pSectionHdr += Marshal.SizeOf<ImageSectionHeader>();
			}

			return arr;
		}
	}

}