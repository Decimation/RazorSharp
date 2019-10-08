using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using NativeSharp.Kernel;
using RazorSharp.CoreClr;
using RazorSharp.Memory.Allocation;
using RazorSharp.Memory.Enums;
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

		/// <summary>
		/// Represents a <c>null</c> pointer.
		/// <seealso cref="IntPtr.Zero"/>
		/// </summary>
		public static readonly Pointer<byte> Nullptr = null;

		#region Calculation

		public static int FullSize<T>(int elemCnt) => Unsafe.SizeOf<T>() * elemCnt;

		#endregion


		/// <summary>
		/// Zeros the memory of <paramref name="value"/>
		/// </summary>
		/// <param name="value">Value to zero</param>
		/// <typeparam name="T">Type of <paramref name="value"/></typeparam>
		public static void Clear<T>(ref T value)
		{
			var ptr = Unsafe.AddressOf(ref value);
			ptr.Clear();
		}


		#region Alloc / free
		
		public static AllocationManager Allocator { get; } = new AllocationManager(Allocators.Local);

		public static void Destroy<T>(ref T value)
		{
			if (!RuntimeInfo.IsStruct(value)) {
				int           size = Unsafe.SizeOf(value, SizeOfOptions.Data);
				Pointer<byte> ptr  = Unsafe.AddressOfFields(ref value);
				ptr.ClearBytes(size);
			}
			else {
				value = default;
			}
		}

		#endregion


		#region Read / Write

		public static T ReadCurrentProcessMemory<T>(Pointer<byte> lpBaseAddress)
		{
			return ReadProcessMemory<T>(Process.GetCurrentProcess(), lpBaseAddress);
		}

		public static T ReadProcessMemory<T>(Process proc, Pointer<byte> lpBaseAddress)
		{
			T   t    = default;
			int size = Unsafe.SizeOf<T>();
			var ptr  = Unsafe.AddressOf(ref t);

			Kernel32.ReadProcessMemory(proc, lpBaseAddress.Address, ptr.Address, size);

			return t;
		}

		public static void WriteCurrentProcessMemory<T>(Pointer<byte> lpBaseAddress, T value)
		{
			WriteProcessMemory(Process.GetCurrentProcess(), lpBaseAddress, value);
		}

		public static void WriteProcessMemory<T>(Process proc, Pointer<byte> lpBaseAddress, T value)
		{
			int dwSize = Unsafe.SizeOf<T>();
			var ptr    = Unsafe.AddressOf(ref value);

			Kernel32.WriteProcessMemory(proc, lpBaseAddress.Address, ptr.Address, dwSize);
		}

		#endregion
	}
}