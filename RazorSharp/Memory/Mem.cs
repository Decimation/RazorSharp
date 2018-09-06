#region

#region

using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using RazorInvoke;
using RazorInvoke.Libraries;
using RazorSharp.CLR;
using RazorSharp.CLR.Structures;
using RazorSharp.Pointers;
using RazorSharp.Utilities;

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
		private const  byte Int32Bits       = 32;
		internal const int  BytesInKilobyte = 1024;

		#region Swap

		public static void Swap<T>(void* a, void* b)
		{
			T aval = CSUnsafe.Read<T>(a);
			T bval = CSUnsafe.Read<T>(b);
			CSUnsafe.Write(a, bval);
			CSUnsafe.Write(b, aval);
		}

		public static void Swap<T>(ref T a, ref T b)
		{
			T buf = a;
			a = b;
			b = buf;
		}

		#endregion

		#region Array operations

		public static byte[] ReadBytes(Pointer<byte> p, int byteOffset, int size)
		{
			byte[] rg = new byte[size];
			fixed (byte* b = rg) {
				Copy(b, byteOffset, p, size);
			}

			return rg;
		}

		public static void WriteBytes(Pointer<byte> dest, byte[] src)
		{
			for (int i = 0; i < src.Length; i++) {
				dest[i] = src[i];
			}
		}

		#endregion

		#region Bits

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ReadBit(int b, int bitIndex)
		{
			return (b & (1 << bitIndex)) != 0;
		}

		public static bool ReadBit(uint b, int bitIndex)
		{
			return ReadBit((int) b, bitIndex);
		}

		public static int ReadBits(int b, int bitIndexBegin, int bitLen)
		{
			if (bitLen > Int32Bits) {
				throw new Exception();
			}

			bool[] bits = new bool[bitLen];
			for (int i = 0; i < bitLen; i++)
				bits[i] = ReadBit(b, bitIndexBegin + i);

			BitArray bitArray = new BitArray(bits);
			int[]    array    = new int[1];
			bitArray.CopyTo(array, 0);
			return array[0];
		}

		public static int ReadBits(uint b, int bitIndexBegin, int bitLen)
		{
			return ReadBits((int) b, bitIndexBegin, bitLen);
		}

		#endregion

		#region Read / write



		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Pointer<T> ReadPointer<T>(Pointer<byte> ptr, long byteOffset)
		{
			return *(IntPtr*) PointerUtils.Add(ptr, byteOffset);
		}

		public static void ForceWrite<T>(Pointer<byte> p, int byteOffset, T t)
		{
			IntPtr hProc = Kernel32.OpenProcess(Process.GetCurrentProcess(), Enumerations.ProcessAccessFlags.All);

			int    lpNumberOfBytesWritten = 0;
			int    size                   = Unsafe.SizeOf<T>();
			IntPtr targetPtr              = PointerUtils.Add(p, byteOffset).Address;

//			bool a = Kernel32.VirtualProtect(targetPtr, (IntPtr) size, Enumerations.MemoryProtection.ExecuteReadWrite, out _);
//			RazorContract.Assert(a);

			bool b = Kernel32.WriteProcessMemory(hProc, targetPtr,
				Unsafe.AddressOf(ref t), size, ref lpNumberOfBytesWritten);

			RazorContract.Assert(b);
			RazorContract.Assert(lpNumberOfBytesWritten == size);

			Kernel32.CloseHandle(hProc);
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

		public static ref T AsRef<T>(Pointer<byte> p, int byteOffset = 0)
		{
			return ref CSUnsafe.AsRef<T>(PointerUtils.Add(p, byteOffset).ToPointer());
		}

		#endregion


		#region Zero

		public static void Zero(IntPtr ptr, int length)
		{
			Zero(ptr.ToPointer(), length);
		}

		public static void Zero(void* ptr, int length)
		{
			byte* memptr = (byte*) ptr;
			for (int i = 0; i < length; i++) {
				memptr[i] = 0;
			}
		}

		#endregion

		#region Stack

		/// <summary>
		///     Determines whether a variable is on the current thread's stack.
		/// </summary>
		public static bool IsOnStack<T>(ref T t)
		{
			return IsOnStack(Unsafe.AddressOf(ref t));
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
		public static IntPtr StackBase => Kernel32.GetCurrentThreadStackLimits().high;

		/// <summary>
		///     Stack Limit / Ceiling of stack (low address)
		/// </summary>
		public static IntPtr StackLimit => Kernel32.GetCurrentThreadStackLimits().low;

		/// <summary>
		///     Should equal <c>4 MB</c> for 64-bit and <c>1 MB</c> for 32-bit
		/// </summary>
		public static long StackSize => StackBase.ToInt64() - StackLimit.ToInt64();

		internal static void StackInit<T>(ref byte* b)
		{
			// ObjHeader
			Zero(b, sizeof(ObjHeader));

			// MethodTable*
			b += sizeof(MethodTable*);
			Pointer<MethodTable> mt  = Runtime.MethodTableOf<T>();
			Pointer<MethodTable> pMt = b;
			pMt.Write(mt);
		}

		#endregion


		/// <summary>
		///     Checks whether an address is in range.
		/// </summary>
		/// <param name="highest">The end address</param>
		/// <param name="p">Address to check</param>
		/// <param name="lowest">The start address</param>
		/// <returns><c>true</c> if the address is in range; <c>false</c> otherwise</returns>
		public static bool IsAddressInRange(IntPtr highest, IntPtr p, IntPtr lowest)
		{
			// return m_CacheStackLimit < addr && addr <= m_CacheStackBase;
			// if (!((object < g_gc_highest_address) && (object >= g_gc_lowest_address)))

			return p.ToInt64() < highest.ToInt64() && p.ToInt64() >= lowest.ToInt64();

//			return max.ToInt64() < p.ToInt64() && p.ToInt64() <= min.ToInt64();
		}


		/// <summary>
		///     Allocates basic reference types in the unmanaged heap.
		///     <para>
		///         Once you are done using the memory, dispose using <see cref="Marshal.FreeHGlobal" /> or <see cref="Free" />
		///     </para>
		/// </summary>
		/// <typeparam name="T">
		///     Type to allocate; cannot be <c>string</c> or an array type (for that, use
		///     <see cref="AllocUnmanaged{T}" />.)
		/// </typeparam>
		/// <returns>A double indirection pointer to the unmanaged instance.</returns>
		public static Pointer<T> AllocUnmanagedInstance<T>() where T : class
		{
			Trace.Assert(!typeof(T).IsArray, "Use AllocUnmanaged for arrays");
			Trace.Assert(typeof(T) != typeof(string));


			// Minimum size required for an instance
			int baseSize = Unsafe.BaseInstanceSize<T>();

			// We'll allocate extra bytes (+ IntPtr.Size) for a pointer and write the address of
			// the unmanaged "instance" there, as the CLR can only interpret
			// reference types as a pointer.
			Pointer<byte>        alloc       = AllocUnmanaged<byte>(baseSize + IntPtr.Size);
			Pointer<MethodTable> methodTable = Runtime.MethodTableOf<T>();

			// Write the pointer in the extra allocated bytes,
			// pointing to the MethodTable* (skip over the extra pointer and the ObjHeader)
			alloc.Write(alloc.Address + sizeof(MethodTable*) * 2);

			// Write the ObjHeader
			// (this'll already be zeroed, but this is just self-documentation)
			// +4 int (sync block)
			// +4 int (padding, x64)
			alloc.Write(0L, 1);

			// Write the MethodTable
			// Managed pointers point to the MethodTable* in the GC heap
			alloc.Write(methodTable, 2);


			return alloc.Reinterpret<T>();
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
		///         Once you are done using the memory, dispose using <see cref="Marshal.FreeHGlobal" /> or <see cref="Free" />
		///     </para>
		/// </summary>
		/// <typeparam name="T">Element type to allocate</typeparam>
		/// <returns>A pointer to the allocated memory</returns>
		public static Pointer<T> AllocUnmanaged<T>(int elemCnt = 1)
		{
			RazorContract.Requires(elemCnt > 0, "elemCnt <= 0");
			int    size  = Unsafe.SizeOf<T>() * elemCnt;
			IntPtr alloc = Marshal.AllocHGlobal(size);
			Zero(alloc, size);

			return alloc;
		}


		/// <summary>
		///     <para>Frees memory allocated from <see cref="AllocUnmanaged{T}" /> using <see cref="Marshal.FreeHGlobal" /></para>
		///     <para>The memory is zeroed before it is freed.</para>
		/// </summary>
		/// <param name="p">Pointer to allocated memory</param>
		public static void Free(IntPtr p)
		{
			// AllocHGlobal is a wrapper of LocalAlloc
			uint size = Kernel32.LocalSize(p.ToPointer());
			Zero(p, (int) size);
			Marshal.FreeHGlobal(p);
		}

		#region Copy

		public static void Copy<T>(Pointer<T> dest, int startOfs, Pointer<T> src, int elemCnt)
		{
			for (int i = startOfs; i < elemCnt; i++) {
				dest[i] = src[i];
			}
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