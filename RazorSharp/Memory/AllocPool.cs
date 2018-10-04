#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using RazorSharp.Common;
using RazorSharp.Pointers;
using static RazorSharp.Unsafe;

#endregion

namespace RazorSharp.Memory
{

	// todo: revise

	/// <summary>
	///     <see cref="AllocPool" /> keeps track of allocated <see cref="Pointer{T}" />s.
	///     <para>Operations in this class work even if the pointer is offset from the original allocated address.</para>
	///     <example>
	///         <para>
	///             For example, if an allocated pointer is
	///             incremented one byte from its original address, you could still <see cref="Free{T}" /> it,
	///             whereas you must use <see cref="Marshal.FreeHGlobal" /> with the original, untouched address
	///             returned by <see cref="Marshal.AllocHGlobal(int)" />.
	///         </para>
	///     </example>
	/// </summary>
	public static class AllocPool
	{
		/// <summary>
		///     List of <see cref="Range" />s
		/// </summary>
		private static readonly IList<Range> s_rgPool;

		static AllocPool()
		{
			s_rgPool = new List<Range>();
		}

		public static Pointer<T> Alloc<T>(int elemCnt = 1)
		{
			Range rg = new Range(Mem.AllocUnmanaged<T>(elemCnt).Address, elemCnt * SizeOf<T>());
			s_rgPool.Add(rg);
			return rg.LowAddr;
		}

		public static Pointer<T> ReAlloc<T>(Pointer<T> ptr, int elemCnt = 1)
		{
			Pointer<T> orig  = GetOrigin(ptr);
			int        index = IndexOf(orig.Address);

			s_rgPool.RemoveAt(index);
			Range rg = new Range(Mem.ReAllocUnmanaged<byte>(orig.Address, elemCnt).Address, elemCnt * SizeOf<T>());
			s_rgPool.Add(rg);
			return rg.LowAddr;
		}

		public static void Free<T>(Pointer<T> ptr)
		{
			Trace.Assert(IsAllocated(ptr));

			Range  rg     = GetRange(ptr.Address);
			IntPtr origin = rg.LowAddr;

			Mem.Zero(origin, rg.Size);
			Mem.Free((Pointer<byte>) origin);
			s_rgPool.Remove(rg);
		}

		private static int IndexOf(IntPtr p)
		{
			for (int i = 0; i < s_rgPool.Count; i++) {
				if (s_rgPool[i].IsAddrInRange(p)) {
					return i;
				}
			}

			return -1;
		}

		public static bool IsAllocated<T>(Pointer<T> ptr)
		{
			return IndexOf(ptr.Address) != -1;
		}

		public static int GetOffset<T>(Pointer<T> ptr)
		{
			return PointerUtils.OffsetIndex<T>(GetOrigin(ptr).Address, ptr.Address);
		}

		private static Range GetRange(IntPtr p)
		{
			int index = IndexOf(p);
			if (index != -1) {
				return s_rgPool[index];
			}

			throw new Exception($"Pointer {Hex.ToHex(p)} is either out of bounds, not allocated, or not in pool");
		}

		public static int GetLength<T>(Pointer<T> ptr)
		{
			return GetRange(ptr.Address).Size / SizeOf<T>();
		}

		public static int GetSize<T>(Pointer<T> ptr)
		{
			return GetRange(ptr.Address).Size;
		}

		public static Pointer<T> GetOrigin<T>(Pointer<T> ptr)
		{
			return GetRange(ptr.Address).LowAddr;
		}

		/// <summary>
		///     Represents the address range of allocated pointers
		/// </summary>
		private struct Range
		{

			private IntPtr HighAddr => LowAddr + Size;

			internal IntPtr LowAddr { get; }

			internal int Size { get; }

			internal Range(IntPtr pLoAddr, int cb)
			{
				LowAddr = pLoAddr;
				Size    = cb;
			}

			internal bool IsAddrInRange(IntPtr p)
			{
				return Mem.IsAddressInRange(HighAddr, p, LowAddr);
			}


		}
	}

}