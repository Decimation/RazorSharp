using System;
using System.Linq;
using RazorSharp.Memory.Pointers;
using RazorSharp.Native.Win32;
using RazorSharp.Native.Win32.Structures;

namespace RazorSharp.Memory
{
	public static class MemInfo
	{
		/// <summary>
		///     Checks whether an address is in range.
		/// </summary>
		/// <param name="hi">The end address</param>
		/// <param name="p">Address to check</param>
		/// <param name="lo">The start address</param>
		/// <returns><c>true</c> if the address is in range; <c>false</c> otherwise</returns>
		public static bool IsAddressInRange(Pointer<byte> hi, Pointer<byte> p, Pointer<byte> lo)
		{
			return p < hi && p >= lo;
		}

		public static bool Is64Bit => IntPtr.Size == sizeof(long);

		public static bool IsInUnmanagedHeap(Pointer<byte> ptr)
		{
			ProcessHeapEntry[] heaps = HeapApi.GetHeapEntries();

			return heaps.Any(heap => ptr == heap.lpData);
		}

		public static bool IsReadable(Pointer<byte> ptr)
		{
			var page = Kernel32.VirtualQuery(ptr.Address);
			return page.IsReadable;
		}

		public static bool IsValid(Pointer<byte> ptr)
		{
			// Obviously can't be null
			if (ptr.IsNull) {
				return false;
			}

			var page = Kernel32.VirtualQuery(ptr.Address);

			if (!page.IsAccessible) {
				return false;
			}

			var info = new AddressInfo(ptr);

			return info.IsAllocated || info.IsInHeap || info.IsInModule || info.IsInPage ||
			       info.IsInSegment || info.IsOnStack || info.IsInUnmanagedHeap;
		}

		#region Stack
		
		/// <summary>
		///     Determines whether a variable is on the current thread's stack.
		/// </summary>
		public static bool IsOnStack<T>(ref T t)
		{
			return IsOnStack(Unsafe.AddressOf(ref t).Address);
		}

		public static bool IsOnStack(Pointer<byte> ptr)
		{
//			(IntPtr low, IntPtr high) bounds = Kernel32.GetCurrentThreadStackLimits();
//			return RazorMath.Between(((IntPtr) v).ToInt64(), bounds.low.ToInt64(), bounds.high.ToInt64(), true);

			// https://github.com/dotnet/coreclr/blob/c82bd22d4bab4369c0989a1c2ca2758d29a0da36/src/vm/threads.h
			// 3620
			return MemInfo.IsAddressInRange(StackBase, ptr.Address, StackLimit);
		}

		/// <summary>
		///     Stack Base / Bottom of stack (high address)
		/// </summary>
		public static Pointer<byte> StackBase => Kernel32.GetCurrentThreadStackLimits().High;

		/// <summary>
		///     Stack Limit / Ceiling of stack (low address)
		/// </summary>
		public static Pointer<byte> StackLimit => Kernel32.GetCurrentThreadStackLimits().Low;

		/// <summary>
		///     Should equal <c>4 MB</c> for 64-bit and <c>1 MB</c> for 32-bit
		/// </summary>
		public static long StackSize => StackBase.ToInt64() - StackLimit.ToInt64();

		#endregion
		
		
	}
}