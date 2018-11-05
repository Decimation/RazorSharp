#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

#endregion

namespace RazorSharp.Common
{

	public static class Enums
	{
		private const string JoinEnumStr = ", ";

		public static string Join(this Enum e, string joinStr = JoinEnumStr)
		{
			return String.Join(joinStr, e.GetFlags());
		}

		internal static object AsNumeric<T>(T e) where T : Enum
		{
			Type underlyingType = Enum.GetUnderlyingType(typeof(T));
			return Misc.InvokeGenericMethod(typeof(Unsafe), "Unbox", underlyingType, null, e);
		}

		public static string CreateString<T>(T e) where T : Enum
		{
			object asNum    = AsNumeric(e);
			string join     = e.ToString();
			string asNumStr = String.Format("({0})", asNum);

			if (asNum.ToString() == join) {
				return asNumStr;
			}

			return join.Length == 0 ? asNumStr : String.Format("{0} {1}", join, asNumStr);
		}

		public static IEnumerable<Enum> GetFlags(this Enum e)
		{
			return Enum.GetValues(e.GetType()).Cast<Enum>().Where(e.HasFlag).Distinct();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static string CreateFlagsString(object num, Enum e)
		{
			string join = e.Join();
			return join == String.Empty ? $"{num}" : $"{num} ({e.Join()})";
		}
	}

}