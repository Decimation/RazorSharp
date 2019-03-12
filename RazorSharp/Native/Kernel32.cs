#region

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using RazorSharp.Native.Enums;
using RazorSharp.Native.Structures;
using RazorSharp.Pointers;
using RazorSharp.Utilities;

#endregion

namespace RazorSharp.Native
{
	/// <summary>
	///     Native P/Invoke for <see cref="KERNEL32_DLL" />
	///     <remarks>
	///         Stolen from RazorInvoke for sake of portability and convenience
	///     </remarks>
	/// </summary>
	internal static unsafe class Kernel32
	{
		internal const int ERROR_INVALID_PARAMETER = 0x57;

		private const string KERNEL32_DLL = "kernel32.dll";


		[DllImport(KERNEL32_DLL)]
		internal static extern uint GetLastError();

		[DllImport(KERNEL32_DLL, SetLastError = true, PreserveSig = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool CloseHandle(IntPtr hObject);

		[DllImport(KERNEL32_DLL, SetLastError = true)]
		internal static extern IntPtr GetCurrentProcess();

		/// <summary>
		///     WOW64 of GetSystemInfo
		/// </summary>
		/// <param name="lpSystemInfo"></param>
		[DllImport(KERNEL32_DLL, SetLastError = true, CharSet = CharSet.Unicode)]
		internal static extern void GetNativeSystemInfo(out SystemInfo lpSystemInfo);

		[DllImport(KERNEL32_DLL)]
		internal static extern IntPtr GetStdHandle(StandardHandles nStdHandle);

		/// <summary>
		///     Retrieves the address of an exported function or variable from the specified dynamic-link library (DLL).
		/// </summary>
		/// <param name="hModule"></param>
		/// <param name="procName"></param>
		/// <returns></returns>
		[DllImport(KERNEL32_DLL, CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
		internal static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

		/// <summary>
		///     Retrieves a module handle for the specified module. The module must have been loaded by the calling process.
		/// </summary>
		/// <param name="lpModuleName"></param>
		/// <returns></returns>
		[DllImport(KERNEL32_DLL, CharSet = CharSet.Auto)]
		internal static extern IntPtr GetModuleHandle(string lpModuleName);

		#region Library

		[DllImport(KERNEL32_DLL, SetLastError = true)]
		internal static extern IntPtr LoadLibrary(string lpFileName);

		[DllImport(KERNEL32_DLL, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool FreeLibrary(IntPtr hModule);

		#endregion

		#region Process

		[DllImport(KERNEL32_DLL, SetLastError = true)]
		internal static extern IntPtr OpenProcess(ProcessAccessFlags processAccess, bool bInheritHandle,
		                                          int                processId);

		internal static IntPtr OpenProcess(Process proc, ProcessAccessFlags flags = ProcessAccessFlags.All)
		{
			return OpenProcess(flags, false, proc.Id);
		}

		internal static IntPtr OpenCurrentProcess(ProcessAccessFlags flags = ProcessAccessFlags.All)
		{
			return OpenProcess(Process.GetCurrentProcess(), flags);
		}

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

		internal static (IntPtr Low, IntPtr High) GetCurrentThreadStackLimits()
		{
			IntPtr l,
			       h;

			GetCurrentThreadStackLimits(&l, &h);
			return (l, h);
		}

		[DllImport(KERNEL32_DLL)]
		internal static extern uint GetCurrentThreadId();

		[DllImport(KERNEL32_DLL)]
		internal static extern IntPtr GetCurrentThread();

		[DllImport(KERNEL32_DLL, SetLastError = true)]
		internal static extern IntPtr OpenThread(uint desiredAccess, bool inheritHandle, uint threadId);


		[DllImport(KERNEL32_DLL, SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint threadId);

		internal static IntPtr OpenThread(ThreadAccess desiredAccess, int threadId)
		{
			return OpenThread(desiredAccess, false, (uint) threadId);
		}

		#endregion


		#region Console

		[DllImport(KERNEL32_DLL)]
		internal static extern bool GetConsoleMode(IntPtr hConsoleHandle, out ConsoleOutputModes lpMode);

		[DllImport(KERNEL32_DLL)]
		internal static extern bool SetConsoleMode(IntPtr hConsoleHandle, ConsoleOutputModes dwMode);

		internal static IntPtr GetConsoleHandle()
		{
			return GetStdHandle(StandardHandles.StdOutputHandle);
		}

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
		///     return value is zero. To get extended error information, call <see cref="GetLastError" />. Possible error values
		///     include <see cref="ERROR_INVALID_PARAMETER" />.
		/// </returns>
		[DllImport(KERNEL32_DLL)]
		internal static extern IntPtr VirtualQuery(IntPtr address, ref MemoryBasicInformation buffer, uint length);

		internal static MemoryBasicInformation VirtualQuery(IntPtr lpAddress)
		{
			var info    = new MemoryBasicInformation();
			var lpValue = VirtualQuery(lpAddress, ref info, (uint) sizeof(MemoryBasicInformation));
			Debug.Assert(lpValue.ToInt64() == sizeof(MemoryBasicInformation));
			return info;
		}


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

		internal static void VirtualProtect(Pointer<byte>        lpAddress, int dwSize, MemoryProtection flNewProtect,
		                                    out MemoryProtection lpflOldProtect)
		{
			Conditions.Assert(VirtualProtect(lpAddress.Address, (uint) dwSize, flNewProtect, out lpflOldProtect));
		}

		#endregion


		#region Read / write

		#region Read

		internal static T ReadCurrentProcessMemory<T>(Pointer<byte> lpBaseAddress)
		{
			return ReadProcessMemory<T>(Process.GetCurrentProcess(), lpBaseAddress);
		}


		internal static byte[] ReadCurrentProcessMemory(Pointer<byte> lpBaseAddress, int cb)
		{
			return ReadProcessMemory(Process.GetCurrentProcess(), lpBaseAddress, cb);
		}


		internal static byte[] ReadProcessMemory(Process proc, Pointer<byte> lpBaseAddress, int cb)
		{
			var hProc = OpenProcess(proc);

			ulong numberOfBytesRead = 0;
			uint  size              = (uint) cb;
			var   mem               = new byte[cb];

			// Read the memory
			Trace.Assert(ReadProcessMemory(hProc, lpBaseAddress.Address, mem, size,
			                               ref numberOfBytesRead));

			Trace.Assert(numberOfBytesRead == size);

			// Close the handle
			Trace.Assert(CloseHandle(hProc));
			return mem;
		}

		internal static T ReadProcessMemory<T>(Process proc, Pointer<byte> lpBaseAddress)
		{
			var   hProc             = OpenProcess(proc);
			T     t                 = default;
			ulong numberOfBytesRead = 0;
			uint  size              = (uint) Unsafe.SizeOf<T>();

			// Read the memory
			Trace.Assert(ReadProcessMemory(hProc, lpBaseAddress.Address, Unsafe.AddressOf(ref t).Address, size,
			                               ref numberOfBytesRead));

			Trace.Assert(numberOfBytesRead == size);

			// Close the handle
			Trace.Assert(CloseHandle(hProc));
			return t;
		}

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
		internal static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr    lpBaseAddress, byte[] lpBuffer,
		                                              ulong  nSize,    ref ulong lpNumberOfBytesRead);

		#endregion

		#region Write

		internal static void WriteCurrentProcessMemory<T>(Pointer<byte> lpBaseAddress, T value)
		{
			WriteProcessMemory(Process.GetCurrentProcess(), lpBaseAddress, value);
		}

		internal static void WriteProcessMemory<T>(Process proc, Pointer<byte> lpBaseAddress, T value)
		{
			var hProc                = OpenProcess(proc);
			int numberOfBytesWritten = 0;
			int dwSize               = Unsafe.SizeOf<T>();

			// Write the memory
			Trace.Assert(WriteProcessMemory(hProc, lpBaseAddress.Address, Unsafe.AddressOf(ref value).Address, dwSize,
			                                ref numberOfBytesWritten));

			Trace.Assert(numberOfBytesWritten == dwSize);

			// Close the handle
			Trace.Assert(CloseHandle(hProc));
		}

		[DllImport(KERNEL32_DLL, SetLastError = true)]
		internal static extern bool WriteProcessMemory(IntPtr  hProcess, IntPtr lpBaseAddress, byte[] lpBuffer,
		                                               int     dwSize,
		                                               ref int lpNumberOfBytesWritten);


		[DllImport(KERNEL32_DLL, SetLastError = true)]
		internal static extern bool WriteProcessMemory(IntPtr  hProcess, IntPtr lpBaseAddress, IntPtr lpBuffer,
		                                               int     dwSize,
		                                               ref int lpNumberOfBytesWritten);

		#endregion

		#endregion
	}
}