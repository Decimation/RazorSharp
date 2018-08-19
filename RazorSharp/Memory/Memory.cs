#region

using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using RazorInvoke.Libraries;
using RazorSharp.CLR;
using RazorSharp.CLR.Structures;
using RazorSharp.Pointers;

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
			T[] arr = new T[elemCount];

			for (int i = 0; i < elemCount; i++) {
				arr[i] = ptr[i];
			}

			return arr;
		}

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

		public static bool IsValid<T>(IntPtr addrOfPtr) where T : class
		{
			if (addrOfPtr == IntPtr.Zero) {
				return false;
			}

			MethodTable* validMethodTable = Runtime.MethodTableOf<T>();
			IntPtr       mt               = Marshal.ReadIntPtr(addrOfPtr);
			MethodTable* readMethodTable  = *(MethodTable**) mt;

			return readMethodTable->Equals(*validMethodTable);
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
			return IsOnStack(Unsafe.AddressOf(ref t).ToPointer());
		}

		public static bool IsOnStack(void* v)
		{
			(IntPtr low, IntPtr high) bounds = Kernel32.GetCurrentThreadStackLimits();
			return RazorMath.Between(((IntPtr) v).ToInt64(), bounds.low.ToInt64(), bounds.high.ToInt64(), true);
		}

		/// <summary>
		///     <para>Allocates a value type in zeroed, unmanaged memory.</para>
		///     <para>Once you are done using the memory, dispose using <see cref="Marshal.FreeHGlobal" /></para>
		/// </summary>
		/// <typeparam name="T">Value type to allocate</typeparam>
		/// <returns>A pointer to the allocated memory</returns>
		public static Pointer<T> AllocUnmanaged<T>(int elemCnt = 1)
		{
			Trace.Assert(elemCnt > 0, "elemCnt <= 0");
			int    size  = Unsafe.SizeOf<T>() * elemCnt;
			IntPtr alloc = Marshal.AllocHGlobal(size);
			Zero(alloc, size);

			return alloc;
		}


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