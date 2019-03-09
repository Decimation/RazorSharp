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
		public static int ReadBits(int number, int nBits, int bitPos)
		{
			return ((1 << nBits) - 1) & (number >> bitPos);
		}

		/**
		 * #define GETMASK(index, size) (((1 << (size)) - 1) << (index))
		 * #define READFROM(data, index, size) (((data) & GETMASK((index), (size))) >> (index))
		 * #define WRITETO(data, index, size, value) ((data) = ((data) & (~GETMASK((index), (size)))) | ((value) << (index)))
		 */

		private static int GetMask(int index, int size)
		{
			return (((1 << (size)) - 1) << (index));
		}

		public static int ReadFrom(int data, int index, int size)
		{
			return (((data) & GetMask((index), (size))) >> (index));
		}

		public static int WriteTo(int data, int index, int size, int value)
		{
			return ((data) = ((data) & (~GetMask((index), (size)))) | ((value) << (index)));
		}
	}
}