#region

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using RazorSharp.Native.Enums;
using RazorSharp.Pointers;

#endregion

namespace RazorSharp.Native
{

	/// <summary>
	/// Native P/Invoke.
	/// <remarks>
	/// Stolen from RazorInvoke for sake of portability and convenience
	/// </remarks>
	///
	/// </summary>
	public static unsafe class Kernel32
	{

		private const string Kernel32Dll = "kernel32.dll";

		#region OpenProcess

		[DllImport(Kernel32Dll, SetLastError = true)]
		public static extern IntPtr OpenProcess(ProcessAccessFlags processAccess, bool bInheritHandle,
			int processId);

		public static IntPtr OpenProcess(Process proc, ProcessAccessFlags flags = ProcessAccessFlags.All)
		{
			return OpenProcess(flags, false, proc.Id);
		}

		public static IntPtr OpenCurrentProcess(ProcessAccessFlags flags = ProcessAccessFlags.All)
		{
			return OpenProcess(Process.GetCurrentProcess(), flags);
		}

		#endregion


		[DllImport(Kernel32Dll)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool CloseHandle(IntPtr hObject);

		/// <summary>
		///     Equals <see cref="Process.Id" />
		/// </summary>
		/// <param name="process"></param>
		/// <returns></returns>
		[DllImport(Kernel32Dll)]
		public static extern uint GetProcessId(IntPtr process);


		[DllImport(Kernel32Dll)]
		public static extern IntPtr GetStdHandle(StandardHandles nStdHandle);

		public static IntPtr GetConsoleHandle()
		{
			return GetStdHandle(StandardHandles.StdOutputHandle);
		}

		#region Virtual

		/// <summary>
		///     <para>https://msdn.microsoft.com/en-us/library/windows/desktop/aa366887(v=vs.85).aspx</para>
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
		[DllImport(Kernel32Dll, SetLastError = true)]
		public static extern IntPtr VirtualAlloc(IntPtr lpAddress, UIntPtr dwSize,
			AllocationType flAllocationType, MemoryProtection flProtect);

		[DllImport(Kernel32Dll)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool VirtualFree(IntPtr lpAddress, uint dwSize, FreeTypes flFreeType);

		[DllImport(Kernel32Dll)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool VirtualProtect([In] IntPtr lpAddress, uint dwSize, MemoryProtection flNewProtect,
			[Out] out MemoryProtection lpflOldProtect);

		#endregion

		[DllImport(Kernel32Dll, SetLastError = true)]
		private static extern void GetCurrentThreadStackLimits(IntPtr* low, IntPtr* high);

		[DllImport(Kernel32Dll)]
		public static extern bool GetConsoleMode(IntPtr hConsoleHandle, out ConsoleOutputModes lpMode);

		[DllImport(Kernel32Dll)]
		public static extern bool SetConsoleMode(IntPtr hConsoleHandle, ConsoleOutputModes dwMode);


		#region Read / write

		public static void WriteProcessMemory<T>(Process proc, Pointer<byte> lpBaseAddress, T value)
		{
			IntPtr hProc                = OpenProcess(proc);
			int    numberOfBytesWritten = 0;
			int    dwSize               = Unsafe.SizeOf<T>();

			// Write the memory
			Trace.Assert(WriteProcessMemory(hProc, lpBaseAddress.Address, Unsafe.AddressOf(ref value), dwSize,
				ref numberOfBytesWritten));

			Trace.Assert(numberOfBytesWritten == dwSize);

			// Close the handle
			Trace.Assert(CloseHandle(hProc));
		}

		public static T ReadProcessMemory<T>(Process proc, Pointer<byte> lpBaseAddress)
		{
			IntPtr hProc             = OpenProcess(proc);
			T      t                 = default;
			ulong  numberOfBytesRead = 0;
			uint   size              = (uint) Unsafe.SizeOf<T>();

			// Read the memory
			Trace.Assert(ReadProcessMemory(hProc, lpBaseAddress.Address, Unsafe.AddressOf(ref t), size,
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
		[DllImport(Kernel32Dll)]
		public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, IntPtr lpBuffer, ulong nSize,
			ref ulong lpNumberOfBytesRead);

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
		[DllImport(Kernel32Dll)]
		public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, ulong nSize,
			ref ulong lpNumberOfBytesRead);

		[DllImport(Kernel32Dll, SetLastError = true)]
		public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, IntPtr lpBuffer, int dwSize,
			ref int lpNumberOfBytesWritten);

		#endregion

		[DllImport(Kernel32Dll, CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
		public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

		[DllImport(Kernel32Dll, CharSet = CharSet.Auto)]
		public static extern IntPtr GetModuleHandle(string lpModuleName);

		public static (IntPtr Low, IntPtr High) GetCurrentThreadStackLimits()
		{
			IntPtr l,
			       h;

			GetCurrentThreadStackLimits(&l, &h);
			return (l, h);
		}


	}

}