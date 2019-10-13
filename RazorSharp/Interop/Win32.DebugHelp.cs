using System;
using System.Runtime.InteropServices;
using System.Security;
using RazorSharp.Interop.Enums;
using RazorSharp.Interop.Structures;
using RazorSharp.Interop.Utilities;
using RazorSharp.Memory;

namespace RazorSharp.Interop
{
	using As = MarshalAsAttribute;
	using Types = UnmanagedType;

	internal static unsafe partial class Win32
	{
		/// <summary>
		/// Functions from <see cref="DBGHELP_DLL"/>
		/// </summary>
		internal static unsafe class DebugHelp
		{
			/// <summary>
			///     https://github.com/Microsoft/DbgShell/blob/master/DbgProvider/internal/Native/DbgHelp.cs
			/// </summary>
			private const string DBGHELP_DLL = "DbgHelp.dll";


			#region Abstraction

			internal static void SymInitialize(IntPtr hProcess) => SymInitialize(hProcess, IntPtr.Zero, false);


			internal static ulong SymLoadModuleEx(IntPtr hProc, IntPtr hFile, string img, string mod, ulong dllBase,
			                                    uint   fileSize)
			{
				return SymLoadModuleEx(hProc, hFile, img, mod, dllBase,
				                       fileSize, IntPtr.Zero, default);
			}


			internal static string GetSymbolName(IntPtr sym)
			{
				var pSym = (SymbolInfo*) sym;
				return Mem.ReadString(&pSym->Name, (int) pSym->NameLen);
			}

			internal static Symbol GetSymbol(IntPtr hProc, string name)
			{
				int sz = (int) (Symbol.StructureSize + Symbol.MAX_SYM_NAME * sizeof(byte)
				                                     + sizeof(ulong) - 1 / sizeof(ulong));

				var byteBuffer = stackalloc byte[sz];
				var buffer     = (SymbolInfo*) byteBuffer;

				buffer->SizeOfStruct = (uint) Symbol.StructureSize;
				buffer->MaxNameLen   = Symbol.MAX_SYM_NAME;


				if (SymFromName(hProc, name, (IntPtr) buffer)) {
					var firstChar = &buffer->Name;
					var symName   = Mem.ReadString(firstChar, (int) buffer->NameLen);
					return new Symbol(buffer, symName);
				}

				throw new Exception(String.Format("Symbol \"{0}\" not found", name));
			}

			#endregion

			#region Sym misc

			[SuppressUnmanagedCodeSecurity]
			[DefaultDllImportSearchPaths(DllImportSearchPath.LegacyBehavior)]
			[DllImport(DBGHELP_DLL, SetLastError = true, CharSet = CharSet.Unicode)]
			[return: As(Types.Bool)]
			private static extern bool SymInitialize(IntPtr                hProcess,
			                                         IntPtr                userSearchPath,
			                                         [As(Types.Bool)] bool fInvadeProcess);


			[SuppressUnmanagedCodeSecurity]
			[DefaultDllImportSearchPaths(DllImportSearchPath.LegacyBehavior)]
			[DllImport(DBGHELP_DLL, SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = nameof(SymCleanup))]
			[return: As(Types.Bool)]
			internal static extern bool SymCleanup(IntPtr hProcess);

			/// <summary>
			///     SYM_ENUMERATESYMBOLS_CALLBACK
			/// </summary>
			/// <param name="symInfo">SYMBOL_INFO</param>
			[return: As(Types.Bool)]
			internal delegate bool SymEnumSymbolsCallback(IntPtr symInfo, uint symbolSize, IntPtr pUserContext);

			#endregion

			#region Sym enum types

			[DefaultDllImportSearchPaths(DllImportSearchPath.LegacyBehavior)]
			[DllImport(DBGHELP_DLL, SetLastError = true, CharSet = CharSet.Unicode,
				EntryPoint                        = "SymEnumTypesByNameW")]
			[return: As(Types.Bool)]
			private static extern bool SymEnumTypesByName(IntPtr                 hProcess,
			                                              ulong                  modBase,
			                                              string                 mask,
			                                              SymEnumSymbolsCallback callback,
			                                              IntPtr                 pUserContext);

			[SuppressUnmanagedCodeSecurity]
			[DefaultDllImportSearchPaths(DllImportSearchPath.LegacyBehavior)]
			[DllImport(DBGHELP_DLL, SetLastError = true, CharSet = CharSet.Unicode)]
			[return: As(Types.Bool)]
			private static extern bool SymEnumTypes(IntPtr                 hProcess,
			                                        ulong                  modBase,
			                                        SymEnumSymbolsCallback callback,
			                                        IntPtr                 pUserContext);

			#endregion

			#region Sym enum symbols

			[DefaultDllImportSearchPaths(DllImportSearchPath.LegacyBehavior)]
			[DllImport(DBGHELP_DLL, SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "SymEnumSymbolsW")]
			[return: As(Types.Bool)]
			internal static extern bool SymEnumSymbols(IntPtr                 hProcess,
			                                           ulong                  modBase,
			                                           string                 mask,
			                                           SymEnumSymbolsCallback callback,
			                                           IntPtr                 pUserContext);

			#endregion

			#region Sym options

			[DllImport(DBGHELP_DLL)]
			internal static extern SymbolOptions SymGetOptions();

			[DllImport(DBGHELP_DLL)]
			internal static extern SymbolOptions SymSetOptions(SymbolOptions options);

			#endregion

			#region Sym from

			[DllImport(DBGHELP_DLL)]
			[return: As(Types.Bool)]
			private static extern bool SymFromName(IntPtr hProcess, IntPtr name, IntPtr pSymbol);

			[DllImport(DBGHELP_DLL)]
			[SuppressUnmanagedCodeSecurity]
			[return: As(Types.Bool)]
			private static extern bool SymFromName(IntPtr hProcess, string name, IntPtr pSymbol);


			[DllImport(DBGHELP_DLL, SetLastError = true)]
			[return: As(Types.Bool)]
			private static extern bool SymFromAddr(IntPtr hProc, ulong addr, ulong* displacement, SymbolInfo* pSym);

			#endregion

			#region Sym module

			[DllImport(DBGHELP_DLL)]
			private static extern ulong SymLoadModule64(IntPtr hProc, IntPtr h, string p, string s, ulong baseAddr,
			                                            uint   fileSize);

			
			[DllImport(DBGHELP_DLL, SetLastError = true)]
			private static extern bool SymGetModuleInfo64(IntPtr hProc, ulong qwAddr, IntPtr pModInfo);

			// BOOL SymUnloadModule64(HANDLE hProcess, DWORD64 BaseOfDll)
			[DllImport(DBGHELP_DLL)]
			[return: As(Types.Bool)]
			internal static extern bool SymUnloadModule64(IntPtr hProc, ulong baseAddr);


			[DefaultDllImportSearchPaths(DllImportSearchPath.LegacyBehavior)]
			[DllImport(DBGHELP_DLL, SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "SymLoadModuleExW")]
			private static extern ulong SymLoadModuleEx(IntPtr hProcess,   IntPtr hFile,     string imageName,
			                                            string moduleName, ulong  baseOfDll, uint   dllSize,
			                                            IntPtr data,       uint   flags);

			#endregion
		}
	}
}