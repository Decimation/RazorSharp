namespace RazorSharp.Memory.Pointers
{
	public enum StringTypes
	{
		/// <summary>
		///     (1-byte) (<see cref="sbyte" />, <see cref="byte"/>) string
		/// <example>
		/// 
		/// <list type="bullet">
		/// <item>
		///<description><c>const char*</c></description>
		/// </item>
		/// 
		///  <item>
		///<description><c>LPCUTF8*</c></description>
		/// </item>
		/// </list>
		/// </example>
		/// </summary>
		ANSI,

		/// <summary>
		///     (2-byte) (<see cref="char" />, <see cref="short" />, <see cref="ushort" /> ) string
		/// <example>
		/// <list type="bullet">
		/// <item>
		///<description><c>const wchar_t*</c></description>
		/// </item>
		/// <item>
		///<description><c>const char16_t*</c></description>
		/// </item>
		/// </list>
		/// </example>
		/// </summary>
		UNI,
		
		CHAR32
	}
}