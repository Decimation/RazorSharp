using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using RazorSharp.CoreClr;
using RazorSharp.CoreClr.Meta;
using SimpleSharp.Strings;
using SimpleSharp.Strings.Formatting;
using SimpleSharp.Utilities;

namespace RazorSharp.Memory.Pointers
{
	public static class Handle
	{
		internal static string ToStringSafe<T>(Pointer<T> ptr)
		{
			// todo: rewrite this

			if (ptr.IsNull)
				return StringConstants.NULL_STR;

			var mt = (MetaType) typeof(T);

			if (mt.IsInteger)
				return String.Format(Handle.VAL_FMT, ptr.Reference, Hex.TryCreateHex(ptr.Reference));

			/* Special support for C-string */
//			if (IsCharPointer())
//				return ReadString(StringTypes.UNI);

			/*if (typeof(T) == typeof(sbyte)) {
				return inst.ReadString(StringTypes.AnsiStr);
			}*/


			if (!Runtime.Info.IsStruct<T>()) {
				Pointer<byte> heapPtr = ptr.ReadPointer();
				string        valueStr;

				if (heapPtr.IsNull) {
					valueStr = StringConstants.NULL_STR;
				}
				else {
					if (mt.IsIListType && !mt.IsString)
						valueStr = $"[{((IEnumerable) ptr.Reference).AutoJoin()}]";
					else
						valueStr = ptr.Reference == null ? StringConstants.NULL_STR : ptr.Reference.ToString();
				}

				return String.Format(Handle.VAL_FMT, valueStr, heapPtr.ToString(Handle.FORMAT_PTR));
			}


			return ptr.Reference.ToString();
		}

		internal static string FormatPointer<T>(Pointer<T> ptr, string format, IFormatProvider formatProvider)
		{
			if (String.IsNullOrEmpty(format))
				format = DefaultFormat;

			if (formatProvider == null)
				formatProvider = CultureInfo.CurrentCulture;

			format = format.ToUpperInvariant();

			return format switch
			{
				FORMAT_ARRAY => FormatArray(),
				FORMAT_INT => ptr.ToInt64().ToString(),
				FORMAT_OBJ => ToStringSafe(ptr),
				FORMAT_PTR => Hex.ToHex(ptr.ToInt64()),
				FORMAT_BOTH => FormatBoth(),
				_ => ToStringSafe(ptr)
			};

			string FormatBoth()
			{
				string thisStr = ToStringSafe(ptr);

				string typeName = typeof(T).ContainsAnyGenericParameters()
					? SystemFormatting.GenericName(typeof(T))
					: typeof(T).Name;

				string typeNameDisplay = Pointer<T>.IsCharPointer() ? CHAR_PTR : typeName;

				return String.Format("{0} @ {1}: {2}", typeNameDisplay, Hex.ToHex(ptr.Address),
				                     thisStr.Contains(Environment.NewLine)
					                     ? Environment.NewLine + thisStr
					                     : thisStr);
			}

			string FormatArray()
			{
				var rg = new string[ArrayCount];

				for (int i = 0; i < rg.Length; i++) {
					rg[i] = ToStringSafe(ptr.AddressOfIndex(i));
				}

				return rg.SimpleJoin();
			}
		}

		#region Format specifiers

		/// <summary>
		///     Object (<see cref="P:RazorSharp.Memory.Pointers.Pointer`1.Reference" />).
		///     <list type="bullet">
		///         <item>
		///             <description>
		///                 If the <see cref="Pointer{T}" />'s type is <see cref="char" />, it will be
		///                 returned as a C-string represented as a <see cref="string" />.
		///             </description>
		///         </item>
		///         <item>
		///             <description>
		///                 If the <see cref="Pointer{T}" />'s type is a reference type, its string representation will be
		///                 returned along with its heap pointer in <see cref="FORMAT_PTR"/> format.
		///             </description>
		///         </item>
		///         <item>
		///             <description>
		///                 If the <see cref="Pointer{T}" />'s type is an <see cref="IList{T}" /> type, its
		///                 contents will be returned along with its heap pointer in <see cref="FORMAT_PTR"/> format.
		///             </description>
		///         </item>
		///         <item>
		///             <description>
		///                 If the <see cref="Pointer{T}" />'s type is a number type, its value will be returned as well its
		///                 value in <see cref="Hex.ToHex(long, HexOptions)" /> format.
		///             </description>
		///         </item>
		///     </list>
		/// </summary>
		private const string FORMAT_OBJ = "O";

		/// <summary>
		/// Displays <see cref="ArrayCount"/> elements
		/// </summary>
		private const string FORMAT_ARRAY = "RG";

		/// <summary>
		///     64-bit integer
		/// </summary>
		private const string FORMAT_INT = "N";

		/// <summary>
		///     Both <see cref="FORMAT_PTR" /> and <see cref="Handle.FORMAT_OBJ" />
		/// </summary>
		private const string FORMAT_BOTH = "B";

		/// <summary>
		///     <para>
		///         Pointer (<see cref="P:RazorSharp.Memory.Pointers.Pointer`1.Address" />) in
		///         <see cref="Hex.ToHex(IntPtr, HexOptions)" /> format
		///     </para>
		/// </summary>
		private const string FORMAT_PTR = "P";

		#endregion

		#region Other

		private const string CHAR_PTR = "Char*";

		private const string VAL_FMT = "{0} ({1})";

		#endregion


		/// <summary>
		/// Default format specifier
		/// </summary>
		public static string DefaultFormat { get; set; } = FORMAT_PTR;

		/// <summary>
		/// Number of elements to display when using <see cref="FORMAT_ARRAY"/>
		/// </summary>
		public static int ArrayCount { get; set; } = 6;
	}
}