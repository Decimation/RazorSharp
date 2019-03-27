#region

#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using RazorSharp.CoreClr;
using RazorSharp.CoreClr.Structures;
using RazorSharp.Native;
using RazorSharp.Native.Enums;
using RazorSharp.Pointers;
using RazorSharp.Utilities;
using Serilog.Context;

#endregion

// ReSharper disable ConvertToAutoProperty

#endregion

namespace RazorSharp.Memory
{
	#region

	using CSUnsafe = System.Runtime.CompilerServices.Unsafe;

	#endregion


	/// <summary>
	///     Provides functions for interacting with memory.
	/// </summary>
	public static unsafe class Mem
	{
		/// <summary>
		/// Bytes in kilobytes
		/// </summary>
		internal const int BYTES_IN_KB = 1024;

		public static bool Is64Bit {
			get { return IntPtr.Size == sizeof(long); }
		}

		#region Zero

		public static void Zero<T>(ref T t)
		{
			Zero(Unsafe.AddressOf(ref t).Address, Unsafe.SizeOf<T>());
		}

		public static void Zero(Pointer<byte> ptr, int length)
		{
			for (int i = 0; i < length; i++) ptr[i] = 0;
		}

		#endregion


		/// <summary>
		///     Checks whether an address is in range.
		/// </summary>
		/// <param name="hi">The end address</param>
		/// <param name="p">Address to check</param>
		/// <param name="lo">The start address</param>
		/// <returns><c>true</c> if the address is in range; <c>false</c> otherwise</returns>
		public static bool IsAddressInRange(Pointer<byte> hi, Pointer<byte> p, Pointer<byte> lo)
		{
			// return m_CacheStackLimit < addr && addr <= m_CacheStackBase;
			// if (!((object < g_gc_highest_address) && (object >= g_gc_lowest_address)))

			return p.ToInt64() < hi.ToInt64() && p.ToInt64() >= lo.ToInt64();

//			return max.ToInt64() < p.ToInt64() && p.ToInt64() <= min.ToInt64();
		}


		#region Alloc / free

		/// <summary>
		/// Counts the number of allocations (allocated pointers)
		/// </summary>
		internal static int AllocCount { get; private set; }

		internal static bool IsMemoryInUse => AllocCount > 0;

		/// <summary>
		///     Allocates basic reference types in the unmanaged heap.
		///     <para>
		///         Once you are done using the memory, dispose using <see cref="Marshal.FreeHGlobal" />,
		/// 		<see cref="Free{T}(Pointer{T})" />, or <see cref="Free{T}(Pointer{T}, int)" />
		///     </para>
		/// </summary>
		/// <typeparam name="T">
		///     Type to allocate; cannot be <c>string</c> or an array type (for that, use
		///     <see cref="AllocUnmanaged{T}" />.)
		/// </typeparam>
		/// <returns>A double indirection pointer to the unmanaged instance.</returns>
		public static Pointer<T> AllocUnmanagedInstance<T>() where T : class
		{
			Conditions.RequiresTypeNot<Array, T>("Use AllocUnmanaged for arrays");
			Conditions.RequiresTypeNot<string, T>();


			// Minimum size required for an instance
			int baseSize = Unsafe.BaseInstanceSize<T>();

			// We'll allocate extra bytes (+ IntPtr.Size) for a pointer and write the address of
			// the unmanaged "instance" there, as the CLR can only interpret
			// reference types as a pointer.
			Pointer<byte>        alloc       = AllocUnmanaged<byte>(baseSize + IntPtr.Size);
			Pointer<MethodTable> methodTable = typeof(T).GetMethodTable();

			// Write the pointer in the extra allocated bytes,
			// pointing to the MethodTable* (skip over the extra pointer and the ObjHeader)
			alloc.WriteAny(alloc.Address + sizeof(MethodTable*) * 2);

			// Write the ObjHeader
			// (this'll already be zeroed, but this is just self-documentation)
			// +4 int (sync block)
			// +4 int (padding, x64)
			alloc.WriteAny(0L, 1);

			// Write the MethodTable
			// Managed pointers point to the MethodTable* in the GC heap
			alloc.WriteAny(methodTable, 2);


			return alloc.Cast<T>();
		}

		/// <summary>
		///     <para>
		///         Allocates <paramref name="elemCnt" /> elements of type <typeparamref name="T" /> in zeroed, unmanaged memory
		///         using <see cref="Marshal.AllocHGlobal(int)" />.
		///     </para>
		///     <para>
		///         If <typeparamref name="T" /> is a reference type, a managed pointer of type <typeparamref name="T" /> will be
		///         created in unmanaged memory, rather than the instance itself. For that, use
		///         <see cref="AllocUnmanagedInstance{T}" />.
		///     </para>
		///     <para>
		///         Once you are done using the memory, dispose using <see cref="Marshal.FreeHGlobal" />,
		/// 		<see cref="Free{T}(Pointer{T})" />, or <see cref="Free{T}(Pointer{T}, int)" />
		///     </para>
		/// </summary>
		/// <typeparam name="T">Element type to allocate</typeparam>
		/// <returns>A pointer to the allocated memory</returns>
		public static Pointer<T> AllocUnmanaged<T>(int elemCnt = 1)
		{
			Conditions.RequiresUnsigned(elemCnt, nameof(elemCnt));
			int size  = Unsafe.SizeOf<T>() * elemCnt;
			var alloc = Marshal.AllocHGlobal(size);
			Zero(alloc, size);

			AllocCount++;

			return alloc;
		}

		public static Pointer<T> ReAllocUnmanaged<T>(Pointer<T> ptr, int elemCnt = 1)
		{
			return Marshal.ReAllocHGlobal(ptr.Address, (IntPtr) (elemCnt * Unsafe.SizeOf<T>()));
		}

		/// <summary>
		///     <para>Frees memory allocated from <see cref="AllocUnmanaged{T}" /> using <see cref="Marshal.FreeHGlobal" /></para>
		/// </summary>
		/// <param name="p">Pointer to allocated memory</param>
		public static void Free<T>(Pointer<T> p)
		{
//			if (bZero) {
			// AllocHGlobal is a wrapper of LocalAlloc
//				uint size = Kernel32.LocalSize(p.ToPointer());
//				Zero(p, (int) size);
//			}

			Marshal.FreeHGlobal(p.Address);
			AllocCount--;
		}

		public static void Free<T>(Pointer<T> p, int length)
		{
			p.ZeroBytes(length);
			Free(p);
		}

		#region String

		/// <summary>
		/// Allocates a native string from a UTF16 C# string
		/// </summary>
		/// <param name="s">Standard UTF16 C# string</param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static Pointer<byte> AllocString(string s, StringTypes type)
		{
			var           size = s.Length + 1;
			Pointer<byte> ptr  = AllocUnmanaged<byte>(size);
			ptr.WriteString(s, type);
			Conditions.Assert(ptr.ReadString(type) == s);

			return ptr;
		}

		public static void FreeString(Pointer<byte> ptr)
		{
			int size = ptr.ReadUntil(x => x == 0x00) + 1;
			ptr.ZeroBytes(size);
			Free(ptr);
		}

		#endregion

		#region Code

		public static Pointer<byte> AllocCode(byte[] opCodes)
		{
			Kernel32.GetNativeSystemInfo(out var si);

			// VirtualAlloc(nullptr, page_size, MEM_COMMIT, PAGE_READWRITE);

			// @formatter:off
			var alloc = Kernel32.VirtualAlloc(IntPtr.Zero, 
			                                  (UIntPtr) si.PageSize, 
			                                  AllocationType.Commit,
			                                  MemoryProtection.ReadWrite);
			// @formatter:on

			AllocCount++;
			Copy(alloc, opCodes);

			// VirtualProtect(buffer, code.size(), PAGE_EXECUTE_READ, &dummy);

			Conditions.NativeRequire(Kernel32.VirtualProtect(alloc, (uint) opCodes.Length,
			                                                 MemoryProtection.ExecuteRead,
			                                                 out _));


			return alloc;
		}

		public static void FreeCode(Pointer<byte> fn)
		{
			Conditions.NativeRequire(Kernel32.VirtualFree(fn.Address, 0, FreeTypes.Release));
			AllocCount--;
		}

		#endregion

		#endregion

		#region Swap

		public static void Swap<T>(void* a, void* b)
		{
			var aval = CSUnsafe.Read<T>(a);
			var bval = CSUnsafe.Read<T>(b);
			CSUnsafe.Write(a, bval);
			CSUnsafe.Write(b, aval);
		}

		public static void Swap<T>(ref T a, ref T b)
		{
			var buf = a;
			a = b;
			b = buf;
		}

		#endregion

		#region Array operations

		public static byte[] ReadBytes(Pointer<byte> p, int size)
		{
			return ReadBytes(p, 0, size);
		}

		public static byte[] ReadBytes(Pointer<byte> p, int byteOffset, int size)
		{
			var rg = new byte[size];
			fixed (byte* b = rg) {
				Copy(b, byteOffset, p, size);
			}

			return rg;
		}

		public static void WriteBytes(Pointer<byte> dest, byte[] src)
		{
			for (int i = 0; i < src.Length; i++) dest[i] = src[i];
		}

		#endregion


		public static IntPtr OffsetAs<TOriginal, TAs>(IntPtr p, int origElemCnt)
		{
			return p + OffsetCountAs<TOriginal, TAs>(origElemCnt);
		}

		/// <summary>
		///     Calculates the element offset (count) of <paramref name="origElemCnt" /> in terms of <typeparamref name="TAs" />
		/// </summary>
		/// <param name="origElemCnt">Original element count in terms of <typeparamref name="TOriginal" /></param>
		/// <typeparam name="TOriginal">Type of <paramref name="origElemCnt" /></typeparam>
		/// <typeparam name="TAs">Type to return <paramref name="origElemCnt" /> as</typeparam>
		/// <returns></returns>
		public static int OffsetCountAs<TOriginal, TAs>(int origElemCnt)
		{
			int origByteCount = origElemCnt * Unsafe.SizeOf<TOriginal>();
			return origByteCount / Unsafe.SizeOf<TAs>();
		}
		
		#region Read / write

		/// <summary>
		///     Reinterprets <paramref name="value" /> of type <typeparamref name="TFrom" /> as a value of type
		///     <typeparamref name="TTo" />
		/// </summary>
		/// <param name="value">Value to reinterpret</param>
		/// <typeparam name="TFrom">Inherent type</typeparam>
		/// <typeparam name="TTo">Type to reinterpret <typeparamref name="TFrom" /> as</typeparam>
		/// <returns></returns>
		public static TTo ReinterpretCast<TFrom, TTo>(TFrom value)
		{
			Pointer<TFrom> addr = Unsafe.AddressOf(ref value);
			return addr.ReadAny<TTo>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Pointer<T> ReadPointer<T>(Pointer<byte> ptr, long byteOffset)
		{
			return *(IntPtr*) ptr.Add(byteOffset);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Write<T>(Pointer<byte> p, int byteOffset, T t)
		{
			CSUnsafe.Write((p + byteOffset).ToPointer(), t);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T Read<T>(Pointer<byte> p, int byteOffset = 0)
		{
			return CSUnsafe.Read<T>((p + byteOffset).ToPointer());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T AsRef<T>(Pointer<byte> p, int byteOffset = 0)
		{
			return ref CSUnsafe.AsRef<T>((p+ byteOffset).ToPointer());
		}

		#endregion

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
			return IsAddressInRange(StackBase, ptr.Address, StackLimit);
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

		internal static void StackInit<T>(ref byte* b)
		{
			Conditions.Requires(IsOnStack(b));

			// ObjHeader
			Zero(b, sizeof(ObjHeader));

			// MethodTable*
			b += sizeof(MethodTable*);
			Pointer<MethodTable> mt  = typeof(T).GetMethodTable();
			Pointer<MethodTable> pMt = b;
			pMt.WriteAny(mt);
		}

		#endregion

		// todo: the Copy methods should probably be implemented in Pointer for consistency

		#region Copy

		public static void Copy<T>(Pointer<T> dest, int startOfs, Pointer<T> src, int elemCnt)
		{
			for (int i = startOfs; i < elemCnt + startOfs; i++)
				dest[i - startOfs] = src[i];
		}

		public static void Copy<T>(Pointer<T> dest, Pointer<T> src, int elemCnt)
		{
			Copy(dest, 0, src, elemCnt);
		}

		public static void Copy(Pointer<byte> dest, byte[] src)
		{
			fixed (byte* b = src) {
				Copy(dest, 0, b, src.Length);
			}
		}

		public static void Copy<T>(Pointer<T> dest, IEnumerable<T> src)
		{
			dest.WriteAll(src);
		}

		#endregion

		#region Alignment

		/*public static bool IsAligned<T>(int byteAlignment)
		{
			int size = Unsafe.SizeOf<T>();
			return (size & (byteAlignment - 1)) == 0;
		}

		public static bool IsAligned<T>()
		{
			return IsAligned<T>(IntPtr.Size);
		}*/

		public static bool IsAligned(IntPtr p)
		{
			return IsAligned(p, IntPtr.Size);
		}

		private static bool IsAligned(IntPtr p, int byteAlignment)
		{
			return (p.ToInt64() & (byteAlignment - 1)) == 0;
		}

		#endregion
	}
}