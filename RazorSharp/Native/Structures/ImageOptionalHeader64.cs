using System;
using System.Runtime.InteropServices;

namespace RazorSharp.Native.Structures
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct ImageOptionalHeader64
	{
		public UInt16 Magic;
		public Byte   MajorLinkerVersion;
		public Byte   MinorLinkerVersion;
		public UInt32 SizeOfCode;
		public UInt32 SizeOfInitializedData;
		public UInt32 SizeOfUninitializedData;
		public UInt32 AddressOfEntryPoint;
		public UInt32 BaseOfCode;
		public UInt64 ImageBase;
		public UInt32 SectionAlignment;
		public UInt32 FileAlignment;
		public UInt16 MajorOperatingSystemVersion;
		public UInt16 MinorOperatingSystemVersion;
		public UInt16 MajorImageVersion;
		public UInt16 MinorImageVersion;
		public UInt16 MajorSubsystemVersion;
		public UInt16 MinorSubsystemVersion;
		public UInt32 Win32VersionValue;
		public UInt32 SizeOfImage;
		public UInt32 SizeOfHeaders;
		public UInt32 CheckSum;
		public UInt16 Subsystem;
		public UInt16 DllCharacteristics;
		public UInt64 SizeOfStackReserve;
		public UInt64 SizeOfStackCommit;
		public UInt64 SizeOfHeapReserve;
		public UInt64 SizeOfHeapCommit;
		public UInt32 LoaderFlags;
		public UInt32 NumberOfRvaAndSizes;

		public ImageDataDirectory ExportTable;
		public ImageDataDirectory ImportTable;
		public ImageDataDirectory ResourceTable;
		public ImageDataDirectory ExceptionTable;
		public ImageDataDirectory CertificateTable;
		public ImageDataDirectory BaseRelocationTable;
		public ImageDataDirectory Debug;
		public ImageDataDirectory Architecture;
		public ImageDataDirectory GlobalPtr;
		public ImageDataDirectory TLSTable;
		public ImageDataDirectory LoadConfigTable;
		public ImageDataDirectory BoundImport;
		public ImageDataDirectory IAT;
		public ImageDataDirectory DelayImportDescriptor;
		public ImageDataDirectory CLRRuntimeHeader;
		public ImageDataDirectory Reserved;
	}
}