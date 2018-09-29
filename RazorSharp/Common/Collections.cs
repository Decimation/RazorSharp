using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RazorSharp.Pointers;

namespace RazorSharp.Common
{

	public static class Collections
	{
		public static T[] RemoveAt<T>(T[] arr, int index)
		{
			var lsBuf = arr.ToList();
			lsBuf.RemoveAt(index);
			arr = lsBuf.ToArray();
			return arr;
		}

		/// <summary>
		/// Alternative to String.Join; works with collections of collections
		/// </summary>
		/// <param name="list"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public static string ListToString(IList list, ToStringOptions options = ToStringOptions.UseCommas)
		{
			StringBuilder sb = new StringBuilder();

			for (int x = 0; x < list.Count; x++) {
				string str;

				if (list[x] == null) {
					str = PointerSettings.NULLPTR;
					if (x + 1 < list.Count)
						x++;
					else {
						break;
					}
				}

				if (list[x].GetType().IsIListType()) {
					str = $"[{ListToString((IList) list[x])}]";
				}
				else {
					if (options.HasFlag(ToStringOptions.Hex) && list.GetType().GetElementType().IsNumericType()) {
						str = $"{list[x]:X}";

						if (options.HasFlag(ToStringOptions.ZeroPadHex) && str.Length == 1) {
							str = 0 + str;
						}

						if (options.HasFlag(ToStringOptions.PrefixHex)) {
							str = Hex.PrefixString + str;
						}
					}
					else str = list[x].ToString();
				}


				if (options.HasFlag(ToStringOptions.UseCommas)) {
					sb.AppendFormat(x + 1 != list.Count ? "{0}, " : "{0}", str);
				}
				else {
					sb.AppendFormat("{0} ", str);
				}
			}

			if (sb.Length > 1 && sb[sb.Length - 1] == ',') {
				sb.Remove(sb.Length - 2, 1);
			}

			if (sb.Length > 1) {
				if (sb[sb.Length - 1] == ' ') {
					sb.Remove(sb.Length - 1, 1);
				}
			}


			return sb.ToString();
		}

		public static string ToString(byte[] mem, ToStringOptions options = ToStringOptions.ZeroPadHex)
		{
			return ListToString(mem, options);
		}

		public static string ToString<T>(IEnumerable<T> arr, ToStringOptions options = ToStringOptions.UseCommas)
		{
			return ListToString((IList) arr, options);
		}

		public static void RemoveAtRange<T>(this IList<T> src, params int[] indices) where T : class
		{
			var arr = src.ToArray();

			foreach (var i in indices) {
				arr[i] = null;
			}

			src.Clear();
			foreach (var v in arr) {
				if (v != null)
					src.Add(v);
			}
		}

		public static string InlineString(IList list, ToStringOptions options = ToStringOptions.UseCommas)
		{
			StringBuilder sb = new StringBuilder();
			string        current;

			for (int i = 0; i < list.Count; i++) {
				#region Format current element

				if (list[i] == null) {
					current = PointerSettings.NULLPTR;
				}

				else if (options.HasFlagFast(ToStringOptions.Hex) && list[i].GetType().IsNumericType()) {
					current = Hex.TryCreateHex(list[i], options);
				}

				else if (list[i].GetType().IsIListType()) {
					current = $"[{InlineString((IList) list[i], options)}]";
				}

				else current = list[i].ToString();

				#endregion

				#region Separate elements

				if (options.HasFlagFast(ToStringOptions.UseCommas)) {
					sb.AppendFormat(i + 1 != list.Count ? "{0}, " : "{0}", current);
				}
				else {
					sb.AppendFormat("{0} ", current);
				}

				#endregion
			}


			return sb.ToString();
		}

		public static void RemoveAll<T>(ref T[] arr, Predicate<T> match)
		{
			var lsBuf = arr.ToList();
			lsBuf.RemoveAll(match);
			arr = lsBuf.ToArray();
		}
	}

}