using System;
using System.Runtime.InteropServices;

// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace RazorSharp.Native.Structures.ThreadContext
{
	[StructLayout(LayoutKind.Explicit)]
	public unsafe struct Arm64NtNeon128
	{
		[FieldOffset(0)]
		public UInt64 Low;

		[FieldOffset(8)]
		public Int64 High;

		[FieldOffset(0)]
		public fixed double D[2];

		[FieldOffset(0)]
		public fixed float S[4];

		[FieldOffset(0)]
		public fixed UInt16 H[8];

		[FieldOffset(0)]
		public fixed Byte B[16];
	}
}