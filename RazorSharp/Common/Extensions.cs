#region

using System;
using System.Collections;
using System.Runtime.CompilerServices;

#endregion

namespace RazorSharp.Common
{
	public static class Extensions
	{
		private const char CHECK    = '\u2713';
		private const char BALLOT_X = '\u2717';

		private static bool HasInterface(this Type t, string interfaceType)
		{
			return t.GetInterface(interfaceType) != null;
		}

		public static bool IsIntegerType(this Type t)
		{
			switch (Type.GetTypeCode(t)) {
				case TypeCode.Byte:
				case TypeCode.SByte:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Int64:
					return true;
				default:
					return false;
			}
		}

		public static bool IsNumericType(this Type o)
		{
			if (IsIntegerType(o)) return true;
			switch (Type.GetTypeCode(o)) {
				// IsIntegerType tests these cases
				/*case TypeCode.Byte:
				case TypeCode.SByte:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Int64:*/
				case TypeCode.Decimal:
				case TypeCode.Double:
				case TypeCode.Single:
					return true;
				default:
					return false;
			}
		}

		public static bool IsIListType(this Type type)
		{
			return type.HasInterface(nameof(IList));
		}

		public static bool IsEnumerableType(this Type type)
		{
			return type.HasInterface(nameof(IEnumerable));
		}

		/// <summary>
		///     Converts the boolean <paramref name="b" /> to a <see cref="char" /> representation.
		/// </summary>
		/// <param name="b"><see cref="bool" /> value</param>
		/// <returns>
		///     <see cref="CHECK" /> if <paramref name="b" /> is <c>true</c>; <see cref="BALLOT_X" /> if <paramref name="b" />
		///     is <c>false</c>
		/// </returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static char Prettify(this bool b)
		{
			return b ? CHECK : BALLOT_X;
		}

		public static bool IsInterned(this string text)
		{
			return string.IsInterned(text) != null;
		}
	}
}