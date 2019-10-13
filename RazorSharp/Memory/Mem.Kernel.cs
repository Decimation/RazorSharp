using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using RazorSharp.Interop;
using RazorSharp.Memory.Pointers;

namespace RazorSharp.Memory
{
	public static partial class Mem
	{
		public static unsafe class Kernel
		{
			#region Read / Write

			public static T ReadCurrentProcessMemory<T>(Pointer<byte> lpBaseAddress) =>
				ReadProcessMemory<T>(Process.GetCurrentProcess(), lpBaseAddress);

			public static T ReadProcessMemory<T>(Process proc, Pointer<byte> lpBaseAddress)
			{
				T   t    = default;
				int size = Unsafe.SizeOf<T>();
				var ptr  = Unsafe.AddressOf(ref t);

				ReadProcessMemory(proc, lpBaseAddress.Address, ptr.Address, size);

				return t;
			}

			public static void WriteCurrentProcessMemory<T>(Pointer<byte> lpBaseAddress, T value) =>
				WriteProcessMemory(Process.GetCurrentProcess(), lpBaseAddress, value);

			public static void WriteProcessMemory<T>(Process proc, Pointer<byte> lpBaseAddress, T value)
			{
				int dwSize = Unsafe.SizeOf<T>();
				var ptr    = Unsafe.AddressOf(ref value);

				WriteProcessMemory(proc, lpBaseAddress.Address, ptr.Address, dwSize);
			}

			#endregion

			#region Read / write raw bytes

			#region Read raw bytes

			public static void ReadProcessMemory(Process       proc,     Pointer<byte> lpBaseAddress,
			                                     Pointer<byte> lpBuffer, int           cb)
			{
				var hProc = Native.Kernel32.OpenProcess(proc);


				// Read the memory
				bool ok = (Native.Kernel32.ReadProcessMemoryInternal(hProc, lpBaseAddress.Address,
				                                                    lpBuffer.Address, cb,
				                                                    out int numberOfBytesRead));

				if (numberOfBytesRead != cb || !ok) {
					throw new Win32Exception();
				}

				// Close the handle
				Native.Kernel32.CloseHandle(hProc);
			}

			public static byte[] ReadProcessMemory(Process proc, Pointer<byte> lpBaseAddress, int cb)
			{
				var mem = new byte[cb];

				fixed (byte* p = mem) {
					ReadProcessMemory(proc, lpBaseAddress, (IntPtr) p, cb);
				}

				return mem;
			}


			#region Current process

			public static byte[] ReadCurrentProcessMemory(Pointer<byte> lpBaseAddress, int cb)
			{
				return ReadProcessMemory(Process.GetCurrentProcess(), lpBaseAddress, cb);
			}

			public static void ReadCurrentProcessMemory(Pointer<byte> lpBaseAddress, Pointer<byte> lpBuffer, int cb)
			{
				ReadProcessMemory(Process.GetCurrentProcess(), lpBaseAddress, lpBuffer, cb);
			}

			#endregion

			#endregion

			#region Write raw bytes

			#region Current process

			public static void WriteCurrentProcessMemory(Pointer<byte> lpBaseAddress, byte[] value)
			{
				WriteProcessMemory(Process.GetCurrentProcess(), lpBaseAddress, value);
			}

			public static void WriteCurrentProcessMemory(Pointer<byte> lpBaseAddress, Pointer<byte> lpBuffer,
			                                             int           dwSize)
			{
				WriteProcessMemory(Process.GetCurrentProcess(), lpBaseAddress, lpBuffer, dwSize);
			}

			#endregion

			public static void WriteProcessMemory(Process proc, Pointer<byte> lpBaseAddress, Pointer<byte> lpBuffer,
			                                      int     dwSize)
			{
				var hProc = Native.Kernel32.OpenProcess(proc);

				// Write the memory
				bool ok = (Native.Kernel32.WriteProcessMemoryInternal(hProc, lpBaseAddress.Address, lpBuffer.Address,
				                                                     dwSize, out int numberOfBytesWritten));


				if (numberOfBytesWritten != dwSize || !ok) {
					throw new Win32Exception();
				}


				// Close the handle
				Native.Kernel32.CloseHandle(hProc);
			}

			public static void WriteProcessMemory(Process proc, Pointer<byte> lpBaseAddress, byte[] value)
			{
				int dwSize = value.Length;

				// Write the memory
				fixed (byte* rg = value) {
					WriteProcessMemory(proc, lpBaseAddress, (IntPtr) rg, dwSize);
				}
			}

			#endregion

			#endregion
		}
	}
}