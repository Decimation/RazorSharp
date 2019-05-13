using System.Runtime.InteropServices;

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
		/// <remarks>Equal to <see cref="UnmanagedType.LPStr"/></remarks>
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
		/// <remarks>Equal to <see cref="UnmanagedType.LPWStr"/></remarks>
		/// </summary>
		UNI,
		
		/// <summary>
		///     (4-byte) (<see cref="int" />, <see cref="uint" />) string
		/// <example>
		/// <list type="bullet">
		/// <item>
		///<description><c>const char32_t*</c></description>
		/// </item>
		/// </list>
		/// </example>
		/// </summary>
		CHAR32
	}
}