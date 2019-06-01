using System;
using System.Runtime.InteropServices;

// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable BuiltInTypeReferenceStyle

namespace RazorSharp.Native.Win32.Structures
{
	using DWORD = UInt32;
	using WORD = UInt16;

	[StructLayout(LayoutKind.Explicit)]
	internal struct ProcessHeapEntry
	{
		[FieldOffset(0)]
		public IntPtr lpData;

		[FieldOffset(8)]
		public DWORD cbData;

		[FieldOffset(12)]
		public byte cbOverhead;

		[FieldOffset(13)]
		public byte iRegionIndex;

		[FieldOffset(14)]
		public WORD wFlags;

		[FieldOffset(16)]
		public Block block;

		[FieldOffset(16)]
		public Region region;
	}
}