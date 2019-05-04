#region

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using RazorSharp.Native.ThreadContext;

#endregion

namespace RazorSharp.Native.Win32
{
	/// <summary>
	///     Native P/Invoke for <see cref="KERNEL32_DLL" />
	/// </summary>
	internal static unsafe partial class Kernel32
	{
		internal const int ERROR_INVALID_PARAMETER = 0x57;

		internal const string KERNEL32_DLL = "kernel32.dll";

		internal static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

		[DllImport(KERNEL32_DLL)]
		internal static extern uint GetLastError();

		[DllImport(KERNEL32_DLL, SetLastError = true, PreserveSig = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool CloseHandle(IntPtr hObject);


		/// <summary>
		///     WOW64 of GetSystemInfo
		/// </summary>
		/// <param name="lpSystemInfo"></param>
		[DllImport(KERNEL32_DLL, SetLastError = true, CharSet = CharSet.Unicode)]
		internal static extern void GetNativeSystemInfo(out SystemInfo lpSystemInfo);


		[DllImport(KERNEL32_DLL)]
		internal static extern uint LocalSize(IntPtr hMem);
		
		#region File

		internal const uint INVALID_FILE_SIZE = 0xFFFFFFFF;

		[DllImport(KERNEL32_DLL, SetLastError = true, CharSet = CharSet.Auto)]
		public static extern IntPtr CreateFile(
			string                                       fileName,
			[MarshalAs(UnmanagedType.U4)] FileAccess     fileAccess,
			[MarshalAs(UnmanagedType.U4)] FileShare      fileShare,
			IntPtr                                       securityAttributes, // optional SECURITY_ATTRIBUTES structure can be passed
			[MarshalAs(UnmanagedType.U4)] FileMode       creationDisposition,
			[MarshalAs(UnmanagedType.U4)] FileAttributes flagsAndAttributes,
			IntPtr                                       template);

		// DWORD GetFileSize(
		// 	HANDLE  hFile,
		// 	LPDWORD lpFileSizeHigh
		// );
		[DllImport(KERNEL32_DLL)]
		public static extern uint GetFileSize(IntPtr hFile, ushort* lpFileSizeHigh);

		#endregion

		

		#region Process

		[DllImport(KERNEL32_DLL, SetLastError = true)]
		internal static extern IntPtr GetCurrentProcess();

		/// <summary>
		///     Equals <see cref="Process.Id" />
		/// </summary>
		[DllImport(KERNEL32_DLL)]
		internal static extern uint GetProcessId(IntPtr process);


		[DllImport(KERNEL32_DLL, SetLastError = true, CharSet = CharSet.Unicode)]
		internal static extern IntPtr OpenProcess(ProcessAccess dwDesiredAccess, bool bInheritHandle, uint processId);

		#endregion

		#region Thread

		[DllImport(KERNEL32_DLL, SetLastError = true)]
		private static extern void GetCurrentThreadStackLimits(IntPtr* low, IntPtr* high);

		[DllImport(KERNEL32_DLL)]
		internal static extern uint GetCurrentThreadId();

		[DllImport(KERNEL32_DLL)]
		internal static extern IntPtr GetCurrentThread();

		[DllImport(KERNEL32_DLL, SetLastError = true, CharSet = CharSet.Unicode)]
		internal static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint threadId);

		[DllImport(KERNEL32_DLL, SetLastError = true)]
		internal static extern bool GetThreadContext(IntPtr hThread, ref Context lpContext);

		// Get context of thread x64, in x64 application
		[DllImport(KERNEL32_DLL, SetLastError = true)]
		internal static extern bool GetThreadContext(IntPtr hThread, ref Context64 lpContext);

		[DllImport(KERNEL32_DLL)]
		internal static extern bool SetThreadContext(IntPtr hThread, ref Context64 lpContext);

		[DllImport(KERNEL32_DLL)]
		internal static extern uint SuspendThread(IntPtr hThread);

		[DllImport(KERNEL32_DLL)]
		internal static extern uint ResumeThread(IntPtr hThread);

		#endregion


		#region Console

		[DllImport(KERNEL32_DLL)]
		internal static extern IntPtr GetStdHandle(StandardHandles nStdHandle);

		[DllImport(KERNEL32_DLL)]
		internal static extern bool GetConsoleMode(IntPtr hConsoleHandle, out ConsoleOutputModes lpMode);

		[DllImport(KERNEL32_DLL)]
		internal static extern bool SetConsoleMode(IntPtr hConsoleHandle, ConsoleOutputModes dwMode);

		#endregion


		#region Virtual

		/// <summary>
		///     Retrieves information about a range of pages in the virtual address space of the calling process.
		///     <para>
		///         <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/aa366902(v=vs.85).aspx"> Doc </a>
		///     </para>
		/// </summary>
		/// <param name="address">
		///     A pointer to the base address of the region of pages to be queried. This value is rounded down to the next page
		///     boundary. To determine the size of a page on the host computer, use the <see cref="GetNativeSystemInfo" />
		///     function. If <paramref name="address" /> specifies an address above the highest memory address accessible to the
		///     process, the function fails with <see cref="ERROR_INVALID_PARAMETER" />.
		/// </param>
		/// <param name="buffer">
		///     A pointer to a <see cref="MemoryBasicInformation" /> structure in which information about the specified page
		///     range is returned.
		/// </param>
		/// <param name="length">The size of the buffer pointed to by the <paramref name="buffer" /> parameter, in bytes.</param>
		/// <returns>
		///     The return value is the actual number of bytes returned in the information buffer. If the function fails, the
		///     return value is zero. To get extended error information, call <see cref="Marshal.GetLastWin32Error" />.
		///     Possible error values include <see cref="ERROR_INVALID_PARAMETER" />.
		/// </returns>
		[DllImport(KERNEL32_DLL)]
		internal static extern IntPtr VirtualQuery(IntPtr address, ref MemoryBasicInformation buffer, uint length);


		/// <summary>
		///     <para>
		///         <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/aa366887(v=vs.85).aspx"> Doc </a>
		///     </para>
		///     Reserves, commits, or changes the state of a region of pages in the virtual address space of the calling process.
		///     Memory allocated by this function is automatically initialized to zero.
		/// </summary>
		/// <param name="lpAddress">
		///     The starting address of the region to allocate. If the memory is being reserved, the specified
		///     address is rounded down to the nearest multiple of the allocation granularity. If the memory is already reserved
		///     and is being committed, the address is rounded down to the next page boundary. To determine the size of a page and
		///     the allocation granularity on the host computer, use the GetSystemInfo function. If this parameter is NULL, the
		///     system determines where to allocate the region.
		/// </param>
		/// <param name="dwSize">
		///     The size of the region, in bytes. If the <paramref name="lpAddress" /> parameter is NULL, this
		///     value is rounded up to the next page boundary. Otherwise, the allocated pages include all pages containing one or
		///     more bytes in the range from <paramref name="lpAddress" /> to <paramref name="lpAddress" /> +
		///     <paramref name="dwSize" />. This means that a 2-byte range straddling a page
		///     boundary causes both pages to be included in the allocated region.
		/// </param>
		/// <param name="flAllocationType"></param>
		/// <param name="flProtect"></param>
		/// <returns>
		///     If the function succeeds, the return value is the base address of the allocated region of pages. If the
		///     function fails, the return value is NULL. To get extended error information, call GetLastError.
		/// </returns>
		[DllImport(KERNEL32_DLL, SetLastError = true)]
		internal static extern IntPtr VirtualAlloc(IntPtr         lpAddress,        UIntPtr          dwSize,
		                                           AllocationType flAllocationType, MemoryProtection flProtect);

		[DllImport(KERNEL32_DLL)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool VirtualFree(IntPtr lpAddress, uint dwSize, FreeTypes flFreeType);

		[DllImport(KERNEL32_DLL)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool VirtualProtect([In] IntPtr                lpAddress, uint dwSize,
		                                           MemoryProtection           flNewProtect,
		                                           [Out] out MemoryProtection lpflOldProtect);

		#endregion


		#region Read / write

		#region Read

		/// <summary>
		///     Reads data from an area of memory in a specified process. The entire area to be read must be accessible or the
		///     operation fails.
		/// </summary>
		/// <param name="hProcess"></param>
		/// <param name="lpBaseAddress"></param>
		/// <param name="lpBuffer"></param>
		/// <param name="nSize"></param>
		/// <param name="lpNumberOfBytesRead"></param>
		/// <returns></returns>
		[DllImport(KERNEL32_DLL)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr    lpBaseAddress, IntPtr lpBuffer,
		                                              ulong  nSize,    ref ulong lpNumberOfBytesRead);

		/// <summary>
		///     Reads data from an area of memory in a specified process. The entire area to be read must be accessible or the
		///     operation fails.
		/// </summary>
		/// <param name="hProcess"></param>
		/// <param name="lpBaseAddress"></param>
		/// <param name="lpBuffer"></param>
		/// <param name="nSize"></param>
		/// <param name="lpNumberOfBytesRead"></param>
		/// <returns></returns>
		[DllImport(KERNEL32_DLL)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr    lpBaseAddress, byte[] lpBuffer,
		                                              ulong  nSize,    ref ulong lpNumberOfBytesRead);

		#endregion

		#region Write

		[DllImport(KERNEL32_DLL, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr  lpBaseAddress, byte[] lpBuffer,
		                                               int    dwSize,   ref int lpNumberOfBytesWritten);


		[DllImport(KERNEL32_DLL, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr  lpBaseAddress, IntPtr lpBuffer,
		                                               int    dwSize,   ref int lpNumberOfBytesWritten);

		#endregion

		#endregion
	}
}