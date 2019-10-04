using System;
using System.Runtime.InteropServices;
using RazorSharp.Memory.Pointers;

namespace RazorSharp.Memory.Allocation
{
	/// <summary>
	/// Contains common memory allocators.
	/// </summary>
	public static class Allocators
	{
		/// <summary>
		/// Exposes <c>LocalAlloc</c> functions from <c>Kernel32.dll</c>
		/// (<c>AllocHGlobal</c> functions from <see cref="Marshal"/>)
		/// </summary>
		private sealed class LocalAllocator : IAllocator
		{
			/// <summary>
			/// <see cref="Marshal.AllocHGlobal(int)"/>
			/// </summary>
			public Pointer<byte> Alloc(int size) =>
				Marshal.AllocHGlobal(size);

			/// <summary>
			/// <see cref="Marshal.ReAllocHGlobal"/>
			/// </summary>
			public Pointer<byte> ReAlloc(Pointer<byte> p, int size) =>
				Marshal.ReAllocHGlobal(p.Address, (IntPtr) size);

			/// <summary>
			/// <see cref="Marshal.FreeHGlobal"/>
			/// </summary>
			public void Free(Pointer<byte> p) =>
				Marshal.FreeHGlobal(p.Address);
		}

		
		public static IAllocator Local { get; } = new LocalAllocator();
	}
}