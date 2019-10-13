using System.Runtime.InteropServices;
using RazorSharp.Interop.Enums;
using RazorSharp.Interop.Utilities;

namespace RazorSharp.Interop.Structures
{
	[Native]
	[StructLayout(LayoutKind.Explicit)]
	public struct ImageSectionHeader
	{
		// Grabbed the following 2 definitions from http://www.pinvoke.net/default.aspx/Structures/IMAGE_SECTION_HEADER.html


		[FieldOffset(0)]
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
		public char[] Name;


		[FieldOffset(8)]
		public uint VirtualSize;


		[FieldOffset(12)]
		public uint VirtualAddress;


		[FieldOffset(16)]
		public uint SizeOfRawData;


		[FieldOffset(20)]
		public uint PointerToRawData;


		[FieldOffset(24)]
		public uint PointerToRelocations;


		[FieldOffset(28)]
		public uint PointerToLinenumbers;


		[FieldOffset(32)]
		public ushort NumberOfRelocations;


		[FieldOffset(34)]
		public ushort NumberOfLinenumbers;


		[FieldOffset(36)]
		public DataSectionFlags Characteristics;

		public string Section => new string(Name);
	}
}