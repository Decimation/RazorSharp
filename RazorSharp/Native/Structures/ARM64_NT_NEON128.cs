using System;
using System.Runtime.InteropServices;

namespace RazorSharp.Native.Structures {
	[StructLayout(LayoutKind.Explicit)]
	public unsafe struct ARM64_NT_NEON128
	{
		[FieldOffset(0)]
		private UInt64 Low;

		[FieldOffset(8)]
		private Int64 High;


		[FieldOffset(0)]
		private fixed double D[2];

		[FieldOffset(0)]
		private fixed float S[4];

		[FieldOffset(0)]
		private fixed UInt16 H[8];

		[FieldOffset(0)]
		private fixed Byte B[16];
	}
}