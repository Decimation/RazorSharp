#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using RazorCommon;
using RazorCommon.Strings;
using RazorSharp.Pointers;
using RazorSharp.Utilities;
using static RazorSharp.Unsafe;

#endregion

namespace RazorSharp.Memory
{
	// todo: WIP

	/// <summary>
	///     <see cref="AllocHelper" /> keeps track of allocated <see cref="Pointer{T}" />s.
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
	public static class AllocHelper
	{
		/// <summary>
		///     List of <see cref="Range" />s
		/// </summary>
		private static readonly IList<Range> Pool;

		static AllocHelper()
		{
			Pool = new List<Range>();
		}

		public static Pointer<T> Alloc<T>(int elemCnt = 1)
		{
			var rg = new Range(Mem.AllocUnmanaged<T>(elemCnt).Address, elemCnt * SizeOf<T>());
			Pool.Add(rg);
			return rg.LowAddress;
		}

		public static Pointer<T> ReAlloc<T>(Pointer<T> ptr, int elemCnt = 1)
		{
			Pointer<T> orig  = GetOrigin(ptr);
			int        index = IndexOf(orig.Address);

			Pool.RemoveAt(index);
			var rg = new Range(Mem.ReAllocUnmanaged<byte>(orig.Address, elemCnt).Address, elemCnt * SizeOf<T>());
			Pool.Add(rg);
			return rg.LowAddress;
		}

		public static void Free<T>(Pointer<T> ptr)
		{
			Conditions.Requires(IsAllocated(ptr));

			var rg     = GetRange(ptr.Address);
			var origin = rg.LowAddress;

			Mem.Zero(origin, rg.Size);
			Mem.Free((Pointer<byte>) origin);
			Pool.Remove(rg);
		}

		private static int IndexOf(IntPtr p)
		{
			for (int i = 0; i < Pool.Count; i++)
				if (Pool[i].IsAddrInRange(p))
					return i;

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
			if (index != -1) 
				return Pool[index];

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
			return GetRange(ptr.Address).LowAddress;
		}

		public static Pointer<T> GetLimit<T>(Pointer<T> ptr)
		{
			return GetRange(ptr.Address).HighAddress;
		}

		

		public static void Info<T>(Pointer<T> ptr)
		{
			var table = new ConsoleTable("Info", "Value");

			table.AddRow("Origin", GetOrigin(ptr).ToString("P"));
			table.AddRow("Limit", GetLimit(ptr).ToString("P"));
			table.AddRow("Current address", ptr.ToString("P"));
			table.AddRow("Value", ptr.ToString("O"));
			table.AddRow("Length", GetLength(ptr));
			table.AddRow("Size", GetSize(ptr));
			table.AddRow("Offset", GetOffset(ptr));
			table.AddRow("Allocated", IsAllocated(ptr));


			Console.WriteLine(table.ToMarkDownString());
		}

		/// <summary>
		///     Represents an address range of an allocated pointer
		/// </summary>
		private struct Range
		{
			internal IntPtr HighAddress => LowAddress + Size;

			internal IntPtr LowAddress { get; }

			internal int Size { get; }

			internal Range(IntPtr pLoAddress, int cb)
			{
				LowAddress = pLoAddress;
				Size    = cb;
			}

			internal bool IsAddrInRange(IntPtr p)
			{
				return Mem.IsAddressInRange(HighAddress, p, LowAddress);
			}
		}
	}
}