#region

using System.Runtime.InteropServices;

#endregion

// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable MemberCanBePrivate.Global

namespace RazorSharp.Native.ThreadContext
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct FloatingSaveArea
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