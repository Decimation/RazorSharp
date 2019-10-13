using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using RazorSharp.Native.Enums;
using RazorSharp.Native.Structures;
using SimpleSharp.Utilities;

namespace RazorSharp.Native.Win32
{
	using As = MarshalAsAttribute;
	using Types = UnmanagedType;

	public static unsafe partial class NativeWin32
	{
		public static unsafe class Kernel
		{
			private const string KERNEL32_DLL = "kernel32.dll";

			private const uint INVALID_FILE_SIZE = 0xFFFFFFFF;

			private const int INVALID_VALUE = -1;

			#region Abstraction

			#region File

			public static IntPtr CreateFile(string   fileName, FileAccess     access, FileShare share,
			                                FileMode mode,     FileAttributes attributes)
			{
				return CreateFile(fileName, access, share, IntPtr.Zero, mode,
				                  attributes, IntPtr.Zero);
			}

			public static void GetFileSize(string pFileName, out ulong fileSize)
			{
				var hFile = CreateFile(pFileName,
				                       FileAccess.Read,
				                       FileShare.Read,
				                       FileMode.Open,
				                       0);

				fileSize = (ulong) GetFileSize(hFile, IntPtr.Zero);

				CloseHandle(hFile);
			}

			public static void GetFileParams(string pFileName, out ulong baseAddr, out ulong fileSize)
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

			public static IntPtr OpenProcess(Process proc, ProcessAccess flags = ProcessAccess.All)
			{
				return OpenProcess(flags, false, proc.Id);
			}

			public static IntPtr OpenCurrentProcess(ProcessAccess flags = ProcessAccess.All)
			{
				return OpenProcess(Process.GetCurrentProcess(), flags);
			}

			#region Read / write

			#region Read

			public static void ReadProcessMemory(Process proc, IntPtr lpBaseAddress, IntPtr lpBuffer, int cb)
			{
				var hProc = OpenProcess(proc);


				// Read the memory
				bool ok = (ReadProcessMemoryInternal(hProc, lpBaseAddress,
				                                     lpBuffer, cb, out int numberOfBytesRead));

				if (numberOfBytesRead != cb) {
					throw new Win32Exception();
				}

				// Close the handle
				CloseHandle(hProc);
			}

			public static byte[] ReadProcessMemory(Process proc, IntPtr lpBaseAddress, int cb)
			{
				var mem = new byte[cb];

				fixed (byte* p = mem) {
					ReadProcessMemory(proc, lpBaseAddress, (IntPtr) p, cb);
				}

				return mem;
			}

			public static byte[] ReadCurrentProcessMemory(IntPtr lpBaseAddress, int cb)
			{
				return ReadProcessMemory(Process.GetCurrentProcess(), lpBaseAddress, cb);
			}

			public static void ReadCurrentProcessMemory(IntPtr lpBaseAddress, IntPtr lpBuffer, int cb)
			{
				ReadProcessMemory(Process.GetCurrentProcess(), lpBaseAddress, lpBuffer, cb);
			}

			#endregion

			#region Write

			public static void WriteCurrentProcessMemory(IntPtr lpBaseAddress, byte[] value)
			{
				WriteProcessMemory(Process.GetCurrentProcess(), lpBaseAddress, value);
			}

			public static void WriteCurrentProcessMemory(IntPtr lpBaseAddress, IntPtr value, int dwSize)
			{
				WriteProcessMemory(Process.GetCurrentProcess(), lpBaseAddress, value, dwSize);
			}

			public static void WriteProcessMemory(Process proc, IntPtr lpBaseAddress, IntPtr value, int dwSize)
			{
				var hProc = OpenProcess(proc);

				// Write the memory
				bool ok = (WriteProcessMemoryInternal(hProc, lpBaseAddress, value,
				                                      dwSize, out int numberOfBytesWritten));


				if (numberOfBytesWritten != dwSize) {
					throw new Win32Exception();
				}


				// Close the handle
				CloseHandle(hProc);
			}

			public static void WriteProcessMemory(Process proc, IntPtr lpBaseAddress, byte[] value)
			{
				int dwSize = value.Length;

				// Write the memory
				fixed (byte* rg = value) {
					WriteProcessMemory(proc, lpBaseAddress, (IntPtr) rg, dwSize);
				}
			}

			#endregion

			#endregion

			#endregion

			[DllImport(KERNEL32_DLL, SetLastError = true, PreserveSig = true, EntryPoint = nameof(CloseHandle))]
			[return: As(Types.Bool)]
			private static extern bool CloseHandle(IntPtr hObject);

			[DllImport(KERNEL32_DLL, SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = nameof(OpenProcess))]
			private static extern IntPtr OpenProcess(ProcessAccess dwDesiredAccess, bool bInheritHandle,
			                                         int           processId);

			[DllImport(KERNEL32_DLL, SetLastError = true)]
			public static extern IntPtr GetCurrentProcess();

			#region File

			[DllImport(KERNEL32_DLL, SetLastError = true, CharSet = CharSet.Auto, EntryPoint = nameof(CreateFile))]
			private static extern IntPtr CreateFile(
				string                        fileName,
				[As(Types.U4)] FileAccess     fileAccess,
				[As(Types.U4)] FileShare      fileShare,
				IntPtr                        securityAttributes, // optional SECURITY_ATTRIBUTES structure can be passed
				[As(Types.U4)] FileMode       creationDisposition,
				[As(Types.U4)] FileAttributes flagsAndAttributes,
				IntPtr                        template);

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
			public static extern IntPtr GetModuleHandle(string lpModuleName);


			[DllImport(KERNEL32_DLL, CharSet = CharSet.Ansi, SetLastError = true, EntryPoint = nameof(GetProcAddress))]
			public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

			#endregion

			#region Read / write

			#region Read

			[DllImport(KERNEL32_DLL, EntryPoint = nameof(ReadProcessMemory))]
			[return: As(Types.Bool)]
			private static extern bool ReadProcessMemoryInternal(IntPtr  hProcess, IntPtr lpBaseAddress,
			                                                     IntPtr  lpBuffer, int    nSize,
			                                                     out int lpNumberOfBytesRead);


			//		[DllImport(KERNEL32_DLL, EntryPoint = nameof(ReadProcessMemory))]
			//		[return: As(Types.Bool)]
			//		private static extern bool ReadProcessMemoryInternal(IntPtr hProcess, IntPtr  lpBaseAddress, byte[] lpBuffer,
			//		                                                      int    nSize,    out int lpNumberOfBytesRead);

			#endregion

			#region Write

			//		[DllImport(KERNEL32_DLL, SetLastError = true, EntryPoint = nameof(WriteProcessMemory))]
			//		[return: As(Types.Bool)]
			//		private static extern bool WriteProcessMemoryInternal(IntPtr hProcess, IntPtr  lpBaseAddress, byte[] lpBuffer,
			//		                                               int    dwSize,   out int lpNumberOfBytesWritten);


			[DllImport(KERNEL32_DLL, SetLastError = true, EntryPoint = nameof(WriteProcessMemory))]
			[return: As(Types.Bool)]
			private static extern bool WriteProcessMemoryInternal(IntPtr  hProcess, IntPtr lpBaseAddress,
			                                                      IntPtr  lpBuffer, int    dwSize,
			                                                      out int lpNumberOfBytesWritten);

			#endregion

			#endregion
		}
	}
}