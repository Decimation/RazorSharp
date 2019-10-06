namespace RazorSharp.CoreClr
{
	internal static unsafe class Constants
	{
		/// <summary>
		///     Common value representing an invalid value or a failure
		/// </summary>
		internal const int INVALID_VALUE = -1;

		/// <summary>
		/// Bits per <see cref="int"/> or <see cref="uint"/>
		/// </summary>
		internal const int BITS_PER_DWORD = sizeof(int) * 8;
	}
}