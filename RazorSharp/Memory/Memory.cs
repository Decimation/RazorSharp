#region

#region

using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using RazorInvoke;
using RazorInvoke.Libraries;
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


	public static unsafe class Memory
	{
		private const byte Int32Bits = 32;

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

		public static byte[] ReadBytes(IntPtr p, int byteOffset, int size)
		{
			byte[] b = new byte[size];
			for (int i = 0; i < size; i++)
				b[i] = Marshal.ReadByte(p, byteOffset + i);
			return b;
		}

		public static void WriteBytes(IntPtr dest, byte[] src)
		{
			for (int i = 0; i < src.Length; i++)
				Marshal.WriteByte(dest, i, src[i]);
		}

		#endregion

		public static T[] CopyOut<T>(IntPtr addr, int elemCount)
		{
			return CopyOut((Pointer<T>) addr, elemCount);
		}

		public static T[] CopyOut<T>(Pointer<T> ptr, int elemCount)
		{
			return ptr.Copy(0, elemCount);
		}

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

		public static void ForceWrite<T>(IntPtr p, int byteOffset, T t)
		{
			IntPtr hProc = Kernel32.OpenProcess(Process.GetCurrentProcess(), Enumerations.ProcessAccessFlags.All);

			int    lpNumberOfBytesWritten = 0;
			int    size                   = Unsafe.SizeOf<T>();
			IntPtr targetPtr              = PointerUtils.Add(p, byteOffset);

//			bool a = Kernel32.VirtualProtect(targetPtr, (IntPtr) size, Enumerations.MemoryProtection.ExecuteReadWrite, out _);
//			RazorContract.Assert(a);

			bool b = Kernel32.WriteProcessMemory(hProc, targetPtr,
				Unsafe.AddressOf(ref t), size, ref lpNumberOfBytesWritten);

			RazorContract.Assert(b);
			RazorContract.Assert(lpNumberOfBytesWritten == size);

			Kernel32.CloseHandle(hProc);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Write<T>(IntPtr p, int byteOffset, T t)
		{
			CSUnsafe.Write((p + byteOffset).ToPointer(), t);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T Read<T>(IntPtr p, int byteOffset = 0)
		{
			return CSUnsafe.Read<T>((p + byteOffset).ToPointer());
		}

		public static ref T AsRef<T>(IntPtr p, int byteOffset = 0)
		{
			return ref CSUnsafe.AsRef<T>(PointerUtils.Add(p, byteOffset).ToPointer());
		}

		public static T As<T>(object o) where T : class
		{
			return CSUnsafe.As<T>(o);
		}

		public static TTo As<TFrom, TTo>(ref TFrom t)
		{
			return CSUnsafe.As<TFrom, TTo>(ref t);
		}


		#region Zero

		public static void Zero(IntPtr ptr, int length)
		{
			Zero(ptr.ToPointer(), length);
		}

		private static void Zero(void* ptr, int length)
		{
			byte* memptr = (byte*) ptr;
			for (int i = 0; i < length; i++) {
				memptr[i] = 0;
			}
		}

		#endregion

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
		/// Checks whether an address is in range.
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


		/// <summary>
		///     <para>Allocates a value type in zeroed, unmanaged memory using <see cref="Marshal.AllocHGlobal(int)" />.</para>
		///     <para>
		///         If <typeparamref name="T" /> is a reference type, a managed pointer of type <typeparamref name="T" /> will be
		///         created in unmanaged memory.
		///     </para>
		///     <para>
		///         Once you are done using the memory, dispose using <see cref="Marshal.FreeHGlobal" /> or <see cref="Free" />
		///     </para>
		/// </summary>
		/// <typeparam name="T">Value type to allocate</typeparam>
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
	}

}