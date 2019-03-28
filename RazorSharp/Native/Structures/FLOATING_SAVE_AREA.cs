using System.Runtime.InteropServices;

namespace RazorSharp.Native.Structures {
	[StructLayout(LayoutKind.Sequential)]
	public struct FLOATING_SAVE_AREA
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