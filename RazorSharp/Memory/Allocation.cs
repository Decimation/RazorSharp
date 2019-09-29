using System;
using System.Runtime.InteropServices;
using RazorSharp.Memory.Pointers;

namespace RazorSharp.Memory
{
	public static class Allocation
	{
		public static Pointer<byte> AllocHGlobal(int cb) => Marshal.AllocHGlobal(cb);

		public static Pointer<byte> ReAllocHGlobal(Pointer<byte> p, int cb) =>
			Marshal.ReAllocHGlobal(p.Address, (IntPtr) cb);

		public static void FreeHGlobal(Pointer<byte> p) => Marshal.FreeHGlobal(p.Address);
	}
}