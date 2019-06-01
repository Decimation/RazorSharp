#region

using System;
using System.Diagnostics;
using SimpleSharp.Diagnostics;
using RazorSharp.Memory;
using RazorSharp.Memory.Pointers;
using RazorSharp.Native.Win32.Enums;
using RazorSharp.Native.Win32.Structures;

#endregion

namespace RazorSharp.Native.Win32
{
	/// <summary>
	///     Wrapper functions
	/// </summary>
	internal static unsafe partial class Kernel32
	{
		#region Console

		internal static IntPtr GetConsoleHandle()
		{
			return GetStdHandle(StandardHandles.StdOutputHandle);
		}

		#endregion

		#region Thread

		internal static (IntPtr Low, IntPtr High) GetCurrentThreadStackLimits()
		{
			IntPtr l,
			       h;

			GetCurrentThreadStackLimits(&l, &h);
			return (l, h);
		}

		#endregion

		#region Process

		internal static IntPtr OpenProcess(Process proc, ProcessAccess flags = ProcessAccess.All)
		{
			return OpenProcess(flags, false, (uint) proc.Id);
		}

		internal static IntPtr OpenCurrentProcess(ProcessAccess flags = ProcessAccess.All)
		{
			return OpenProcess(Process.GetCurrentProcess(), flags);
		}

		#endregion

		#region Query / Protect

		internal static void VirtualProtect(Pointer<byte>        lpAddress, int dwSize, MemoryProtection flNewProtect,
		                                    out MemoryProtection lpflOldProtect)
		{
			Conditions.Ensure(VirtualProtect(lpAddress.Address, (uint) dwSize, flNewProtect,
			                                 out lpflOldProtect));
		}

		internal static MemoryBasicInformation VirtualQuery(IntPtr lpAddress)
		{
			var info    = new MemoryBasicInformation();
			var lpValue = VirtualQuery(lpAddress, ref info, (uint) sizeof(MemoryBasicInformation));
			Conditions.Ensure(lpValue.ToInt64() == sizeof(MemoryBasicInformation));
			return info;
		}

		#endregion

		#region Read / Write

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
			Conditions.Ensure(ReadProcessMemory(hProc, lpBaseAddress.Address, mem, size, ref numberOfBytesRead));

			Conditions.Ensure(numberOfBytesRead == size);

			// Close the handle
			Conditions.Ensure(CloseHandle(hProc));
			return mem;
		}

		internal static T ReadProcessMemory<T>(Process proc, Pointer<byte> lpBaseAddress)
		{
			var   hProc             = OpenProcess(proc);
			T     t                 = default;
			ulong numberOfBytesRead = 0;
			uint  size              = (uint) Unsafe.SizeOf<T>();

			// Read the memory
			Conditions.Ensure(ReadProcessMemory(hProc, lpBaseAddress.Address,
			                                    Unsafe.AddressOf(ref t).Address,
			                                    size, ref numberOfBytesRead));

			Conditions.Ensure(numberOfBytesRead == size);

			// Close the handle
			Conditions.Ensure(CloseHandle(hProc));
			return t;
		}

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
			Conditions.Ensure(WriteProcessMemory(hProc, lpBaseAddress.Address,
			                                     Unsafe.AddressOf(ref value).Address,
			                                     dwSize, ref numberOfBytesWritten));

			Conditions.Ensure(numberOfBytesWritten == dwSize);

			// Close the handle
			Conditions.Ensure(CloseHandle(hProc));
		}

		#endregion
	}
}