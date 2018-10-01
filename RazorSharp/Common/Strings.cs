#region

using System;
using System.Collections.Generic;

#endregion

namespace RazorSharp.Common
{

	public static class Strings
	{
		/// <summary>
		///     Java substring.
		/// </summary>
		/// <param name="s">
		///     <see cref="string" />
		/// </param>
		/// <param name="beginIndex">Start index</param>
		/// <returns></returns>
		public static string JSubstring(this string s, int beginIndex)
		{
			// simulates Java substring function
			return s.Substring(beginIndex, s.Length);
		}

		/// <summary>
		///     Java substring.
		/// </summary>
		/// <param name="s">
		///     <see cref="string" />
		/// </param>
		/// <param name="beginIndex">Start index</param>
		/// <param name="endIndex">End index, exclusive</param>
		/// <returns></returns>
		public static string JSubstring(this string s, int beginIndex, int endIndex)
		{
			// simulates Java substring function
			int len = endIndex - beginIndex;
			return s.Substring(beginIndex, len);
		}

		/// <summary>
		///     Get string value after [last] <paramref name="a" />
		/// </summary>
		public static string SubstringAfter(this string value, string a)
		{
			int posA = value.LastIndexOf(a, StringComparison.Ordinal);
			if (posA == -1) {
				return String.Empty;
			}

			int adjustedPosA = posA + a.Length;
			return adjustedPosA >= value.Length ? String.Empty : value.Substring(adjustedPosA);
		}

		/// <summary>
		///     Get string value after [first] <paramref name="a" />
		/// </summary>
		public static string SubstringBefore(this string value, string a)
		{
			int posA = value.IndexOf(a, StringComparison.Ordinal);
			return posA == -1 ? String.Empty : value.Substring(0, posA);
		}

		/// <summary>
		///     Get string value between [first] <paramref name="a" /> and [last] <paramref name="b" />.
		/// </summary>
		public static string SubstringBetween(this string value, string a, string b)
		{
			int posA = value.IndexOf(a, StringComparison.Ordinal);
			int posB = value.LastIndexOf(b, StringComparison.Ordinal);
			if (posA == -1) {
				return String.Empty;
			}

			if (posB == -1) {
				return String.Empty;
			}

			int adjustedPosA = posA + a.Length;
			return adjustedPosA >= posB ? String.Empty : value.Substring(adjustedPosA, posB - adjustedPosA);
		}

		public static IEnumerable<int> AllIndexesOf(this string str, string searchString)
		{
			int minIndex = str.IndexOf(searchString, StringComparison.InvariantCulture);
			while (minIndex != -1) {
				yield return minIndex;
				minIndex = str.IndexOf(searchString, minIndex + searchString.Length, StringComparison.InvariantCulture);
			}
		}
	}

}