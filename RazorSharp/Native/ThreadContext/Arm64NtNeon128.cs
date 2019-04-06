#region

using System.Runtime.InteropServices;

#endregion

// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace RazorSharp.Native.ThreadContext
{
	[StructLayout(LayoutKind.Explicit)]
	internal unsafe struct Arm64NtNeon128
	{
		[FieldOffset(0)]
		public ulong Low;

		[FieldOffset(8)]
		public long High;

		[FieldOffset(0)]
		public fixed double D[2];

		[FieldOffset(0)]
		public fixed float S[4];

		[FieldOffset(0)]
		public fixed ushort H[8];

		[FieldOffset(0)]
		public fixed byte B[16];
	}
}