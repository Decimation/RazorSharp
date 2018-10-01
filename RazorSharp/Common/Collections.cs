#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RazorSharp.Pointers;

#endregion

namespace RazorSharp.Common
{

	public static class Collections
	{

		#region Remove

		public static T[] RemoveAt<T>(T[] arr, int index)
		{
			List<T> lsBuf = arr.ToList();
			lsBuf.RemoveAt(index);
			arr = lsBuf.ToArray();
			return arr;
		}


		public static void RemoveAtRange<T>(this IList<T> src, params int[] indices) where T : class
		{
			T[] arr = src.ToArray();

			foreach (int i in indices) {
				arr[i] = null;
			}

			src.Clear();
			foreach (T v in arr) {
				if (v != null) {
					src.Add(v);
				}
			}
		}


		public static void RemoveAll<T>(ref T[] arr, Predicate<T> match)
		{
			List<T> lsBuf = arr.ToList();
			lsBuf.RemoveAll(match);
			arr = lsBuf.ToArray();
		}

		#endregion


		#region ToString

		public static string ToString(byte[] mem, ToStringOptions options = ToStringOptions.ZeroPadHex)
		{
			return ToString(list: mem, options);
		}

		public static string ToString<T>(IEnumerable<T> enumerable, ToStringOptions options = ToStringOptions.UseCommas)
		{
			return ToString((IList) enumerable, options);
		}

		public static string ToString(IList list, ToStringOptions options = ToStringOptions.UseCommas)
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
					current = $"[{ToString((IList) list[i], options)}]";
				}

				else {
					current = list[i].ToString();
				}

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

		#endregion


	}

}