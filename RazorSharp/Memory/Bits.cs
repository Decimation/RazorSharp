#region

using System.Runtime.CompilerServices;

#endregion

namespace RazorSharp.Memory
{
	public static class Bits
	{
		/// <summary>
		///     Reads <paramref name="nBits" /> from <paramref name="number" /> at offset <paramref name="bitPos" />
		/// </summary>
		/// <param name="number"><see cref="System.Int32" /> value to read from</param>
		/// <param name="nBits">Number of bits to read</param>
		/// <param name="bitPos">Beginning offset</param>
		public static int ReadBits(int number, int nBits, int bitPos) => ((1 << nBits) - 1) & (number >> bitPos);

		public static int ReadBits(uint number, int nBits, int bitPos) => ReadBits((int) number, nBits, bitPos);
		
		/**
		 * #define GETMASK(index, size) (((1 << (size)) - 1) << (index))
		 * #define READFROM(data, index, size) (((data) & GETMASK((index), (size))) >> (index))
		 * #define WRITETO(data, index, size, value) ((data) = ((data) & (~GETMASK((index), (size)))) | ((value) << (index)))
		 */
		
		private static int GetMask(int index, int size) => ((1 << size) - 1) << index;

		public static int ReadFrom(int data, int index, int size) => (data & GetMask(index, size)) >> index;

		public static int WriteTo(int data, int index, int size, int value)
		{
			return (data & ~GetMask(index, size)) | (value << index);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ReadBit(int b, int bitIndex) => (b & (1 << bitIndex)) != 0;

		public static bool ReadBit(uint b, int bitIndex) => ReadBit((int) b, bitIndex);
	}
}