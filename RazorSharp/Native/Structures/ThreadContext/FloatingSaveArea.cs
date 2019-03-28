using System.Runtime.InteropServices;

// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable MemberCanBePrivate.Global

namespace RazorSharp.Native.Structures.ThreadContext
{
	[StructLayout(LayoutKind.Sequential)]
	public struct FloatingSaveArea
	{
		public uint ControlWord;
		public uint StatusWord;
		public uint TagWord;
		public uint ErrorOffset;
		public uint ErrorSelector;
		public uint DataOffset;
		public uint DataSelector;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 80)]
		public byte[] RegisterArea;

		public uint Cr0NpxState;
	}
}