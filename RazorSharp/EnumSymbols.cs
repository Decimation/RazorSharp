using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using RazorCommon.Diagnostics;
using RazorSharp.Memory;
using RazorSharp.Native;
using RazorSharp.Native.Structures;
using RazorSharp.Native.Structures.Symbols;
using static RazorSharp.Native.SymTagEnum;
using SymTagEnum = RazorSharp.Native.SymTagEnum;

namespace RazorSharp
{
	public unsafe class EnumSymbols : IDisposable
	{
		private IntPtr m_proc;

		private void ShowSymbolInfo(ulong modBase)
		{
			IMAGEHELP_MODULE64 module64 = default;

			int size = Marshal.SizeOf<IMAGEHELP_MODULE64>();

			Console.WriteLine("Size: {0}", size);
			module64.SizeOfStruct = size;

			var pAlloc = Marshal.AllocHGlobal(size);
			Mem.Zero(pAlloc, size);
			Marshal.StructureToPtr(module64, pAlloc, false);

			bool bRet = DbgHelp.SymGetModuleInfo64(m_proc, modBase, pAlloc);

			NativeFunctions.Call(bRet, nameof(DbgHelp.SymGetModuleInfo64), false);


			// Display information about symbols 

			// Kind of symbols

			module64 = Marshal.PtrToStructure<IMAGEHELP_MODULE64>(pAlloc);
			Marshal.FreeHGlobal(pAlloc);
			
			Console.WriteLine("Sym type: {0}", module64.SymType);

			// Image name

			Console.WriteLine("Img name {0}", module64.ImageName);

			// Loaded image name

			Console.WriteLine("Loaded img name {0}", module64.LoadedImageName);

			// Loaded pdb name

			Console.WriteLine("PDB name {0}", module64.LoadedPdbName);

			// Is debug information unmatched ?
			// (It can only happen if the debug information is contained
			// in a separate file (.DBG or .PDB)

			if (module64.PdbUnmatched || module64.DbgUnmatched) {
				Console.WriteLine("Warning: unmatched symbols");
			}

			// Contents

			// Line numbers available ?

			Console.WriteLine("Line numbers: {0} \n", module64.LineNumbers ? "Available" : "Not available");

			// Global symbols available ?

			Console.WriteLine("Global symbols: {0} \n", module64.GlobalSymbols ? "Available" : "Not available");

			// Type information available ?

			Console.WriteLine("Type information: {0} \n", module64.TypeInfo ? "Available" : "Not available");

			// Source indexing available ?

			Console.WriteLine("Source indexing: {0} \n", module64.SourceIndexed ? "Yes" : "No");

			// Public symbols available ?

			Console.WriteLine("Public symbols: {0} \n", module64.Publics ? "Available" : "Not available");
		}

		private static string TagStr(uint tag)
		{
			switch ((SymTagEnum) tag) {
				case SymTagNull:
					return "Null";
				case SymTagExe:
					return "Exe";
				case SymTagCompiland:
					return "Compiland";
				case SymTagCompilandDetails:
					return "CompilandDetails";
				case SymTagCompilandEnv:
					return "CompilandEnv";
				case SymTagFunction:
					return "Function";
				case SymTagBlock:
					return "Block";
				case SymTagData:
					return "Data";
				case SymTagAnnotation:
					return "Annotation";
				case SymTagLabel:
					return "Label";
				case SymTagPublicSymbol:
					return "PublicSymbol";
				case SymTagUDT:
					return "UDT";
				case SymTagEnum.SymTagEnum:
					return "Enum";
				case SymTagFunctionType:
					return "FunctionType";
				case SymTagPointerType:
					return "PointerType";
				case SymTagArrayType:
					return "ArrayType";
				case SymTagBaseType:
					return "BaseType";
				case SymTagTypedef:
					return "Typedef";
				case SymTagBaseClass:
					return "BaseClass";
				case SymTagFriend:
					return "Friend";
				case SymTagFunctionArgType:
					return "FunctionArgType";
				case SymTagFuncDebugStart:
					return "FuncDebugStart";
				case SymTagFuncDebugEnd:
					return "FuncDebugEnd";
				case SymTagUsingNamespace:
					return "UsingNamespace";
				case SymTagVTableShape:
					return "VTableShape";
				case SymTagVTable:
					return "VTable";
				case SymTagCustom:
					return "Custom";
				case SymTagThunk:
					return "Thunk";
				case SymTagCustomType:
					return "CustomType";
				case SymTagManagedType:
					return "ManagedType";
				case SymTagDimension:
					return "Dimension";
				default:
					return "Unknown";
			}

			return "";
		}

		public EnumSymbols()
		{
			m_proc = Kernel32.GetCurrentProcess();
		}

		private static void ShowSymbolDetails(SymbolInfo* symInfo)
		{
			// Kind of symbol (tag) 
			Console.WriteLine("Symbol: {0}  ", TagStr(symInfo->Tag));

			// Address 
			Console.WriteLine("Address: {0} ", symInfo->Address);

			// Size 
			Console.WriteLine("Size: {0}  ", symInfo->Size);

			// Name 
			Console.WriteLine("Name: {0}", NativeHelp.GetString(&symInfo->Name, symInfo->Name));
		}

		bool MyEnumSymbolsCallback(IntPtr pSymInfo, uint symSize, IntPtr userCtx)
		{
			if (pSymInfo != IntPtr.Zero) {
				ShowSymbolDetails((SymbolInfo*) pSymInfo);
			}

			return true; // continue enumeration
		}

		public void LoadAll(string pFileName)
		{
			string pSearchMask = null;

			uint options = DbgHelp.SymGetOptions();

			// SYMOPT_DEBUG option asks DbgHelp to print additional troubleshooting
			// messages to debug output - use the debugger's Debug Output window
			// to view the messages

			const uint SYMOPT_DEBUG = 0x80000000;
			options |= SYMOPT_DEBUG;

			DbgHelp.SymSetOptions(options);

			bool bRet;

			// Initialize DbgHelp and load symbols for all modules of the current process 


			bRet = DbgHelp.SymInitialize(m_proc, 				// Process handle of the current process
			                             null,   	// No user-defined search path -> use default
			                             false); 	// Do not load symbols for modules in the current process


			do {
				ulong baseAddr = 0;
				ulong fileSize = 0;

				if (!GetFileParams(pFileName, ref baseAddr, ref fileSize)) {
					Console.WriteLine("ERROR");
					break;
				}

				Console.WriteLine("Loading symbols");

				ulong modBase = DbgHelp.SymLoadModule64(
					m_proc,         // Process handle of the current process
					IntPtr.Zero,    // Handle to the module's image file (not needed)
					pFileName,      // Path/name of the file
					null,           // User-defined short name of the module (it can be NULL)
					baseAddr,       // Base address of the module (cannot be NULL if .PDB file is used, otherwise it can be NULL)
					(uint) fileSize // Size of the file (cannot be NULL if .PDB file is used, otherwise it can be NULL)
				);

				NativeFunctions.Call(modBase !=0, nameof(DbgHelp.SymLoadModule64));
				
				if (modBase == 0) {
					break;
				}

				Console.WriteLine("Load addr: {0:X}", modBase);

				// Obtain and display information about loaded symbols 
				ShowSymbolInfo(modBase);

				// Enumerate symbols and display information about them

				Console.WriteLine("Symbols: ");

				var symEnumSuccess = DbgHelp.SymEnumSymbols(
					m_proc,                // Process handle of the current process
					modBase,               			// Base address of the module
					pSearchMask,           			// Mask (NULL -> all symbols)
					MyEnumSymbolsCallback, 			// The callback function
					IntPtr.Zero           // A used-defined context can be passed here, if necessary
				);


				NativeFunctions.Call(symEnumSuccess, nameof(DbgHelp.SymEnumSymbols));

				NativeFunctions.Call(DbgHelp.SymUnloadModule64(m_proc, modBase), nameof(DbgHelp.SymUnloadModule64));
				
			} while (false);

			
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
				Console.WriteLine("CreateFile failed");
				return false;
			}

			fileSize = Kernel32.GetFileSize(hFile, null);

			if (fileSize == Kernel32.INVALID_FILE_SIZE) {
				Console.WriteLine("GetFileSize failed");
			}

			Conditions.Ensure(Kernel32.CloseHandle(hFile));

			return fileSize != Kernel32.INVALID_FILE_SIZE;
		}

		private bool GetFileParams(string pFileName, ref ulong baseAddr, ref ulong fileSize)
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
			}

			return true;
		}

		public void Dispose()
		{
			Kernel32.CloseHandle(m_proc);
		}
	}
}