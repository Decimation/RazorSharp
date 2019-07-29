using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using NativeSharp.Kernel;
using RazorSharp.CoreClr;
using RazorSharp.Memory.Pointers;
using RazorSharp.Model;

// ReSharper disable UnusedParameter.Global

namespace RazorSharp.Memory
{
	/// <summary>
	///     Provides functions for interacting with memory.
	///     <seealso cref="Unsafe" />
	///     <seealso cref="Mem" />
	///     <para></para>
	/// </summary>
	public static unsafe class Mem
	{
		public static bool Is64Bit => IntPtr.Size == sizeof(long) && Environment.Is64BitProcess;

		public static readonly Pointer<byte> Nullptr = null;

		static Mem()
		{
			Allocator = new Allocator(Marshal.AllocHGlobal, 
			                          (ptr, size) => Marshal.ReAllocHGlobal(ptr, (IntPtr) size), 
			                          Marshal.FreeHGlobal);
		}

		#region Calculation

		public static int FullSize<T>(int elemCnt)
		{
			return Unsafe.SizeOf<T>() * elemCnt;
		}

		#endregion

		#region Zero

		public static void Zero<T>(ref T t)
		{
			Zero(Unsafe.AddressOf(ref t).Cast(), Unsafe.SizeOf<T>());
		}

		public static void Zero(Pointer<byte> ptr, int length)
		{
			for (int i = 0; i < length; i++)
				ptr[i] = default;
		}

		#endregion

		#region Alloc / free

		/// <summary>
		/// A <see cref="Allocator"/> using <see cref="Marshal.AllocHGlobal(int)"/>,
		/// <see cref="Marshal.ReAllocHGlobal"/>, and <see cref="Marshal.FreeHGlobal"/>
		/// </summary>
		public static Allocator Allocator { get; }

		public static void Destroy<T>(ref T value)
		{
			if (!RuntimeInfo.IsStruct(value)) {
				int           size = Unsafe.SizeOf(value, SizeOfOptions.Data);
				Pointer<byte> ptr  = Unsafe.AddressOfFields(ref value);
				ptr.ZeroBytes(size);
			}
			else {
				value = default;
			}
		}

		#endregion


		#region Read / Write

		internal static T ReadCurrentProcessMemory<T>(Pointer<byte> lpBaseAddress)
		{
			return ReadProcessMemory<T>(Process.GetCurrentProcess(), lpBaseAddress);
		}

		internal static T ReadProcessMemory<T>(Process proc, Pointer<byte> lpBaseAddress)
		{
			T   t    = default;
			int size = Unsafe.SizeOf<T>();
			var ptr  = Unsafe.AddressOf(ref t);

			Kernel32.ReadProcessMemory(proc, lpBaseAddress.Address, ptr.Address, size);

			return t;
		}

		internal static void WriteCurrentProcessMemory<T>(Pointer<byte> lpBaseAddress, T value)
		{
			WriteProcessMemory(Process.GetCurrentProcess(), lpBaseAddress, value);
		}

		internal static void WriteProcessMemory<T>(Process proc, Pointer<byte> lpBaseAddress, T value)
		{
			int dwSize = Unsafe.SizeOf<T>();
			var ptr    = Unsafe.AddressOf(ref value);

			Kernel32.WriteProcessMemory(proc, lpBaseAddress.Address, ptr.Address, dwSize);
		}

		#endregion

		
	}
}