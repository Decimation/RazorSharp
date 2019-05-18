using System;
using System.Runtime.InteropServices;

// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable BuiltInTypeReferenceStyle

namespace RazorSharp.Native
{
	using DWORD = UInt32;
	using WORD = UInt16;

	[StructLayout(LayoutKind.Sequential)]
	internal struct Region
	{
		public UInt32 dwCommittedSize;
		public UInt32 dwUnCommittedSize;
		public IntPtr lpFirstBlock;
		public IntPtr lpLastBlock;
	}
}