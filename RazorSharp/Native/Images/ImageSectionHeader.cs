#region

using System;
using System.Runtime.InteropServices;
using SimpleSharp.Diagnostics;
using RazorSharp.Memory;

// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable MemberCanBePrivate.Global

#endregion

// ReSharper disable BuiltInTypeReferenceStyle

namespace RazorSharp.Native.Images
{
	#region

	using DWORD = UInt32;
	using WORD = UInt16;

	#endregion

	[StructLayout(LayoutKind.Explicit)]
	internal struct ImageSectionHeader
	{
		static ImageSectionHeader()
		{
			Conditions.Require(Mem.Is64Bit);
		}

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = ImageSectionInfo.IMAGE_SIZEOF_SHORT_NAME)]
		[FieldOffset(0)]
		public string Name;

		[FieldOffset(8)]
		public DWORD PhysicalAddress;

		[FieldOffset(8)]
		public DWORD VirtualSize;

		[FieldOffset(12)]
		public DWORD VirtualAddress;

		[FieldOffset(14)]
		public DWORD SizeOfRawData;

		[FieldOffset(18)]
		public DWORD PointerToRawData;

		[FieldOffset(22)]
		public DWORD PointerToRelocations;

		[FieldOffset(26)]
		public DWORD PointerToLinenumbers;

		[FieldOffset(30)]
		public WORD NumberOfRelocations;

		[FieldOffset(32)]
		public WORD NumberOfLinenumbers;

		[FieldOffset(36)]
		public ImageSectionCharacteristics Characteristics;
	}
}