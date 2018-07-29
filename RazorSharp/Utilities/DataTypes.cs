using System;
using System.Collections.Generic;

namespace RazorSharp.Utilities
{

	internal enum NamingStyles
	{
		/// <summary>
		/// WORD, DWORD, QWORD, etc
		/// </summary>
		Windows,

		/// <summary>
		/// MSVC __int8, __int16, __int32, __int64, etc
		/// </summary>
		Fixed,

		/// <summary>
		/// Byte, Int16, Int32, Int64, etc
		/// </summary>
		CSharp,

		/// <summary>
		/// byte, short, int, long, etc
		/// </summary>
		CSharpKeyword,
	}

	internal static class DataTypes
	{
		private struct NameStyle
		{
			internal string Windows       { get; }
			internal string Fixed         { get; }
			internal string CSharp { get; }
			internal string CSharpKeyword { get; }

			internal NameStyle(string windows, string @fixed, string cSharp, string cSharpKeyword)
			{
				Windows       = windows;
				Fixed         = @fixed;
				CSharp        = cSharp;
				CSharpKeyword = cSharpKeyword;

			}
		}

		public static string GetStyle<T>(NamingStyles style)
		{
			switch (style) {
				case NamingStyles.Windows:
					return Styles[typeof(T)].Windows;
				case NamingStyles.Fixed:
					return Styles[typeof(T)].Fixed;
				case NamingStyles.CSharp:
					return Styles[typeof(T)].CSharp;
				case NamingStyles.CSharpKeyword:
					return Styles[typeof(T)].CSharpKeyword;
				default:
					throw new ArgumentOutOfRangeException(nameof(style), style, null);
			}
		}

		private static readonly Dictionary<Type, NameStyle> Styles;

		static DataTypes()
		{
			Styles = new Dictionary<Type, NameStyle>
			{
				{typeof(byte), new NameStyle("-", "__int8", "Byte", "byte")},

				{typeof(char), new NameStyle("WCHAR", "-", "Char", "char")},

				{typeof(ushort), new NameStyle("WORD", "-", "UInt16","ushort")},
				{typeof(short), new NameStyle("-", "__int16", "Int16", "short")},

				{typeof(uint), new NameStyle("DWORD", "-",  "UInt32","uint")},
				{typeof(int), new NameStyle("-", "__int32", "Int32","int" )},

				{typeof(ulong), new NameStyle("QWORD", "-", "UInt64", "ulong" )},
				{typeof(long), new NameStyle("-", "__int64", "Int64", "long" )},

				{typeof(string), new NameStyle("-", "-", "String", "string")}
			};
		}
	}

}