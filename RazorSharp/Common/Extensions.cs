#region

using System;
using System.Collections;
using System.Runtime.CompilerServices;

#endregion

namespace RazorSharp.Common
{

	public static class Extensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool HasFlagFast(this ToStringOptions value, ToStringOptions flag)
		{
			return (value & flag) != 0;
		}

		private static bool HasInterface(this Type t, string interfaceType)
		{
			return t.GetInterface(interfaceType) != null;
		}

		public static bool IsNumericType(this Type o)
		{
			switch (Type.GetTypeCode(o)) {
				case TypeCode.Byte:
				case TypeCode.SByte:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Int64:
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

		public const char Check   = '\u2713';
		public const char BallotX = '\u2717';

		/// <summary>
		///     Converts the boolean <paramref name="b" /> to a <see cref="char" /> representation.
		/// </summary>
		/// <param name="b"><see cref="bool" /> value</param>
		/// <returns>
		///     <see cref="Check" /> if <paramref name="b" /> is <c>true</c>; <see cref="BallotX" /> if <paramref name="b" />
		///     is <c>false</c>
		/// </returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static char Prettify(this bool b)
		{
			return b ? Check : BallotX;
		}
	}

}