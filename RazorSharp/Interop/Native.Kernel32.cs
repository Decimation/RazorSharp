using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using RazorSharp.Interop.Enums;
using RazorSharp.Interop.Structures;
using RazorSharp.Memory;

namespace RazorSharp.Interop
{
	using As = MarshalAsAttribute;
	using Types = UnmanagedType;

	internal static unsafe partial class Native
	{
		/// <summary>
		/// Functions from <see cref="KERNEL32_DLL"/>
		/// </summary>
		internal static unsafe class Kernel32
		{
			private const string KERNEL32_DLL = "kernel32.dll";


			#region Abstraction

			#region File

			private static IntPtr CreateFile(string   fileName, FileAccess     access, FileShare share,
			                                 FileMode mode,     FileAttributes attributes)
			{
				return CreateFile(fileName, access, share, IntPtr.Zero, mode,
				                  attributes, IntPtr.Zero);
			}

			private static void GetFileSize(string pFileName, out ulong fileSize)
			{
				var hFile = CreateFile(pFileName, FileAccess.Read, FileShare.Read, FileMode.Open, 0);

				fileSize = (ulong) GetFileSize(hFile, IntPtr.Zero);

				CloseHandle(hFile);
			}

			internal static void GetFileParams(string pFileName, out ulong baseAddr, out ulong fileSize)
			{
				// Is it .PDB file ?

				const string PDB_EXT = "pdb";

				if (pFileName.Contains(PDB_EXT)) {
					// Yes, it is a .PDB file 

					// Determine its size, and use a dummy base address 

					// it can be any non-zero value, but if we load symbols 
					// from more than one file, memory regions specified
					// for different files should not overlap
					// (region is "base address + file size")
					baseAddr = 0x10000000;


					GetFileSize(pFileName, out fileSize);
				}
				else {
					// It is not a .PDB file 

					// Base address and file size can be 0 

					baseAddr = 0;
					fileSize = 0;

					throw new NotImplementedException();
				}
			}

			#endregion

			#region Process

			internal static IntPtr OpenProcess(Process proc, ProcessAccess flags = ProcessAccess.All)
			{
				return OpenProcess(flags, false, proc.Id);
			}

			internal static IntPtr OpenCurrentProcess(ProcessAccess flags = ProcessAccess.All)
			{
				return OpenProcess(Process.GetCurrentProcess(), flags);
			}

			#endregion

			#endregion

			[DllImport(KERNEL32_DLL, SetLastError = true, PreserveSig = true, EntryPoint = nameof(CloseHandle))]
			[return: As(Types.Bool)]
			internal static extern bool CloseHandle(IntPtr hObject);

			[DllImport(KERNEL32_DLL, SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = nameof(OpenProcess))]
			private static extern IntPtr OpenProcess(ProcessAccess dwDesiredAccess, bool bInheritHandle,
			                                         int           processId);

			[DllImport(KERNEL32_DLL, SetLastError = true)]
			internal static extern IntPtr GetCurrentProcess();

			#region File

			[DllImport(KERNEL32_DLL, SetLastError = true, CharSet = CharSet.Auto, EntryPoint = nameof(CreateFile))]
			private static extern IntPtr CreateFile(string    fileName,            FileAccess     fileAccess,
			                                        FileShare fileShare,           IntPtr         securityAttributes,
			                                        FileMode  creationDisposition, FileAttributes flagsAndAttributes,
			                                        IntPtr    template);

			[DllImport(KERNEL32_DLL, EntryPoint = nameof(GetFileSize))]
			[return: As(Types.I4)]
			private static extern uint GetFileSize(IntPtr hFile, IntPtr lpFileSizeHigh);

			#endregion

			#region Virtual

			[DllImport(KERNEL32_DLL, EntryPoint = nameof(VirtualQuery))]
			internal static extern IntPtr VirtualQuery(IntPtr                     address,
			                                           ref MemoryBasicInformation buffer,
			                                           int                        length);

			[DllImport(KERNEL32_DLL, EntryPoint = nameof(VirtualProtect))]
			[return: As(Types.Bool)]
			internal static extern bool VirtualProtect(IntPtr               lpAddress, int dwSize,
			                                           MemoryProtection     flNewProtect,
			                                           out MemoryProtection lpflOldProtect);

			#endregion

			#region Module

			[DllImport(KERNEL32_DLL, CharSet = CharSet.Auto, EntryPoint = nameof(GetModuleHandle))]
			internal static extern IntPtr GetModuleHandle(string lpModuleName);

			[DllImport(KERNEL32_DLL, CharSet = CharSet.Ansi, SetLastError = true, EntryPoint = nameof(GetProcAddress))]
			internal static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

			#endregion

			#region Read / write

			#region Read

			[DllImport(KERNEL32_DLL, EntryPoint = nameof(Mem.Kernel.ReadProcessMemory))]
			[return: As(Types.Bool)]
			internal static extern bool ReadProcessMemoryInternal(IntPtr  hProcess, IntPtr lpBaseAddress,
			                                                      IntPtr  lpBuffer, int    nSize,
			                                                      out int lpNumberOfBytesRead);

			#endregion

			#region Write

			[DllImport(KERNEL32_DLL, SetLastError = true, EntryPoint = nameof(Mem.Kernel.WriteProcessMemory))]
			[return: As(Types.Bool)]
			internal static extern bool WriteProcessMemoryInternal(IntPtr  hProcess, IntPtr lpBaseAddress,
			                                                       IntPtr  lpBuffer, int    dwSize,
			                                                       out int lpNumberOfBytesWritten);

			#endregion

			#endregion
		}
	}
}