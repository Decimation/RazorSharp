#region

using System.Runtime.InteropServices;

#endregion

// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable MemberCanBePrivate.Global

namespace RazorSharp.Native.Structures.ThreadContext
{
	/// <summary>
	///     x64
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 16)]
	public struct XSaveFormat64
	{
		public ushort ControlWord;
		public ushort StatusWord;
		public byte   TagWord;
		public byte   Reserved1;
		public ushort ErrorOpcode;
		public uint   ErrorOffset;
		public ushort ErrorSelector;
		public ushort Reserved2;
		public uint   DataOffset;
		public ushort DataSelector;
		public ushort Reserved3;
		public uint   MxCsr;
		public uint   MxCsr_Mask;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
		public M128A[] FloatRegisters;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
		public M128A[] XmmRegisters;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 96)]
		public byte[] Reserved4;
	}
}