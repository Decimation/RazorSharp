namespace RazorSharp.Memory.Pointers
{
	public enum StringTypes
	{
		/// <summary>
		///     LPCUTF8 native string (<see cref="sbyte" /> (1-byte) string)
		/// </summary>
		ANSI,

		/// <summary>
		///     <see cref="char" /> (2-byte) string
		/// </summary>
		UNI
	}
}