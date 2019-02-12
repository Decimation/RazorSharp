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
	}
}