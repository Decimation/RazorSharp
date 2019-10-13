using System.Diagnostics;
using RazorSharp.Memory.Pointers;
using RazorSharp.Native.Win32;

namespace RazorSharp.Memory
{
	public static partial class Mem
	{
		public static class Kernel
		{
			#region Read / Write

			public static T ReadCurrentProcessMemory<T>(Pointer<byte> lpBaseAddress) =>
				ReadProcessMemory<T>(Process.GetCurrentProcess(), lpBaseAddress);

			public static T ReadProcessMemory<T>(Process proc, Pointer<byte> lpBaseAddress)
			{
				T   t    = default;
				int size = Unsafe.SizeOf<T>();
				var ptr  = Unsafe.AddressOf(ref t);

				NativeWin32.Kernel.ReadProcessMemory(proc, lpBaseAddress.Address, ptr.Address, size);

				return t;
			}

			public static void WriteCurrentProcessMemory<T>(Pointer<byte> lpBaseAddress, T value) =>
				WriteProcessMemory(Process.GetCurrentProcess(), lpBaseAddress, value);

			public static void WriteProcessMemory<T>(Process proc, Pointer<byte> lpBaseAddress, T value)
			{
				int dwSize = Unsafe.SizeOf<T>();
				var ptr    = Unsafe.AddressOf(ref value);

				NativeWin32.Kernel.WriteProcessMemory(proc, lpBaseAddress.Address, ptr.Address, dwSize);
			}

			#endregion
		}
	}
}