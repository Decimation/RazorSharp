#region

using System;
using System.Runtime.InteropServices;
using RazorSharp.Native.Enums;
using RazorSharp.Native.Enums.Images;

#endregion

// ReSharper disable BuiltInTypeReferenceStyle

namespace RazorSharp.Native.Structures.Images
{
	#region

	using DWORD = UInt32;
	using WORD = UInt16;

	#endregion

	[StructLayout(LayoutKind.Explicit)]
	public struct ImageSectionHeader
	{
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = ImageSectionInfo.IMAGE_SIZEOF_SHORT_NAME)]
		[FieldOffset(0)]
		public readonly string Name;

		[FieldOffset(8)]
		public readonly DWORD PhysicalAddress;

		[FieldOffset(8)]
		public readonly DWORD VirtualSize;

		[FieldOffset(12)]
		public readonly DWORD VirtualAddress;

		[FieldOffset(14)]
		public readonly DWORD SizeOfRawData;

		[FieldOffset(18)]
		public readonly DWORD PointerToRawData;

		[FieldOffset(22)]
		public readonly DWORD PointerToRelocations;

		[FieldOffset(26)]
		public readonly DWORD PointerToLinenumbers;

		[FieldOffset(30)]
		public readonly WORD NumberOfRelocations;

		[FieldOffset(32)]
		public readonly WORD NumberOfLinenumbers;

		[FieldOffset(36)]
		public readonly ImageSectionCharacteristics Characteristics;
	}
}