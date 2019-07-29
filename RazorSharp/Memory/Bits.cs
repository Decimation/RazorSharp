using System.Runtime.CompilerServices;

namespace RazorSharp.Memory
{
	internal static class Bits
	{
		/// <summary>
		///     Reads <paramref name="bitCount" /> from <paramref name="value" /> at offset <paramref name="bitOfs" />
		/// </summary>
		/// <param name="value"><see cref="int" /> value to read from</param>
		/// <param name="bitOfs">Beginning offset</param>
		/// <param name="bitCount">Number of bits to read</param>
		internal static int ReadBits(int value, int bitOfs, int bitCount)
		{
			return ((1 << bitCount) - 1) & (value >> bitOfs);
		}

		internal static int ReadBits(uint value, int bitOfs, int bitCount)
		{
			return ReadBits((int) value, bitOfs, bitCount);
		}

		/**
		 * #define GETMASK(index, size) (((1 << (size)) - 1) << (index))
		 * #define READFROM(data, index, size) (((data) & GETMASK((index), (size))) >> (index))
		 * #define WRITETO(data, index, size, value) ((data) = ((data) & (~GETMASK((index), (size)))) | ((value) << (index)))
		 */
		
		private static int GetMask(int index, int size) => ((1 << size) - 1) << index;

		internal static int ReadFrom(int data, int index, int size) => (data & GetMask(index, size)) >> index;

		internal static int WriteTo(int data, int index, int size, int value)
		{
			return (data & ~GetMask(index, size)) | (value << index);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static bool ReadBit(int value, int bitOfs) => (value & (1 << bitOfs)) != 0;

		internal static bool ReadBit(uint value, int bitOfs) => ReadBit((int) value, bitOfs);
	}
}