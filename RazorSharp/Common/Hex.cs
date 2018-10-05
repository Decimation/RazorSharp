#region

using System;
using System.Runtime.CompilerServices;
using RazorSharp.Pointers;

#endregion

namespace RazorSharp.Common
{

	[Flags]
	public enum ToStringOptions
	{

		/// <summary>
		///     Separate elements with a comma
		/// </summary>
		UseCommas = 1,

		/// <summary>
		///     Represent the number in hexadecimal format
		/// </summary>
		Hex = 2,


		/// <summary>
		///     Pad single-digit hex with a zero
		/// </summary>
		ZeroPadHex = Hex | 4,


		/// <summary>
		///     Prefix hex with <see cref="RazorSharp.Common.Hex.PrefixString"/>
		/// </summary>
		PrefixHex = Hex | 8,
	}


	/// <summary>
	///     <para>Creates hex representations of pointers</para>
	///     <para>All operations in this class implicitly use <see cref="ToStringOptions.Hex" /></para>
	/// </summary>
	public static unsafe class Hex
	{
		private const string          PrefixString = "0x";
		public static ToStringOptions Options { get; set; }


		static Hex()
		{
			Options = ToStringOptions.PrefixHex;
		}

		public static string ToHex<T>(Pointer<T> ptr)
		{
			return ToHexInternal(ptr.ToInt64());
		}

		public static string ToHex(IntPtr p)
		{
			return ToHex(p.ToInt64());
		}

		public static string ToHex(ulong u)
		{
			return ToHexInternal((long) u);
		}

		public static string ToHex(long l)
		{
			return ToHexInternal(l);
		}

		public static string ToHex(void* v)
		{
			return ToHex((long) v);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static string ToHexInternal(long l)
		{
			string s = l.ToString("X");
			if (Options.HasFlagFast(ToStringOptions.PrefixHex)) {
				s = PrefixString + s;
			}


			return s;
		}


		/// <summary>
		///     Independent from <see cref="ToHexInternal" />. Not for pointers.
		///     Implicitly uses <see cref="ToStringOptions.Hex" />
		/// </summary>
		/// <param name="t"></param>
		/// <param name="options"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static string TryCreateHex<T>(T t, ToStringOptions options = ToStringOptions.PrefixHex)
		{
			string value = null;
			if (t.GetType().IsNumericType()) {
				long l = Int64.Parse(t.ToString());
				value = $"{l:X}";

				if (value.Length == 1 && options.HasFlag(ToStringOptions.ZeroPadHex)) {
					value = 0 + value;
				}


				if (options.HasFlag(ToStringOptions.PrefixHex)) {
					value = PrefixString + value;
					Console.WriteLine("pfx");
				}
			}

			return string.IsNullOrEmpty(value) ? t.ToString() : value;
		}
	}

}