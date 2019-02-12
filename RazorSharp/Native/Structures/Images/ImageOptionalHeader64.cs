#region

using System.Runtime.InteropServices;

#endregion

namespace RazorSharp.Native.Structures.Images
{
	[StructLayout(LayoutKind.Sequential)]
	public struct ImageOptionalHeader64
	{
		/// WORD->unsigned short
		public ushort Magic;

		/// BYTE->unsigned char
		public byte MajorLinkerVersion;

		/// BYTE->unsigned char
		public byte MinorLinkerVersion;

		/// DWORD->unsigned int
		public uint SizeOfCode;

		/// DWORD->unsigned int
		public uint SizeOfInitializedData;

		/// DWORD->unsigned int
		public uint SizeOfUninitializedData;

		/// DWORD->unsigned int
		public uint AddressOfEntryPoint;

		/// DWORD->unsigned int
		public uint BaseOfCode;

		/// ULONGLONG->unsigned __int64
		public ulong ImageBase;

		/// DWORD->unsigned int
		public uint SectionAlignment;

		/// DWORD->unsigned int
		public uint FileAlignment;

		/// WORD->unsigned short
		public ushort MajorOperatingSystemVersion;

		/// WORD->unsigned short
		public ushort MinorOperatingSystemVersion;

		/// WORD->unsigned short
		public ushort MajorImageVersion;

		/// WORD->unsigned short
		public ushort MinorImageVersion;

		/// WORD->unsigned short
		public ushort MajorSubsystemVersion;

		/// WORD->unsigned short
		public ushort MinorSubsystemVersion;

		/// DWORD->unsigned int
		public uint Win32VersionValue;

		/// DWORD->unsigned int
		public uint SizeOfImage;

		/// DWORD->unsigned int
		public uint SizeOfHeaders;

		/// DWORD->unsigned int
		public uint CheckSum;

		/// WORD->unsigned short
		public ushort Subsystem;

		/// WORD->unsigned short
		public ushort DllCharacteristics;

		/// ULONGLONG->unsigned __int64
		public ulong SizeOfStackReserve;

		/// ULONGLONG->unsigned __int64
		public ulong SizeOfStackCommit;

		/// ULONGLONG->unsigned __int64
		public ulong SizeOfHeapReserve;

		/// ULONGLONG->unsigned __int64
		public ulong SizeOfHeapCommit;

		/// DWORD->unsigned int
		public uint LoaderFlags;

		/// DWORD->unsigned int
		public uint NumberOfRvaAndSizes;

		/// IMAGE_DATA_DIRECTORY[16]
		public ImageDataDirectory DataDirectory_0;

		public ImageDataDirectory DataDirectory_1;
		public ImageDataDirectory DataDirectory_2;
		public ImageDataDirectory DataDirectory_3;
		public ImageDataDirectory DataDirectory_4;
		public ImageDataDirectory DataDirectory_5;
		public ImageDataDirectory DataDirectory_6;
		public ImageDataDirectory DataDirectory_7;
		public ImageDataDirectory DataDirectory_8;
		public ImageDataDirectory DataDirectory_9;
		public ImageDataDirectory DataDirectory_10;
		public ImageDataDirectory DataDirectory_11;
		public ImageDataDirectory DataDirectory_12;
		public ImageDataDirectory DataDirectory_13;
		public ImageDataDirectory DataDirectory_14;
		public ImageDataDirectory DataDirectory_15;
	}
}