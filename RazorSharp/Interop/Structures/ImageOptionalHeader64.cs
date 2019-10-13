using System.Runtime.InteropServices;
using RazorSharp.Interop.Utilities;

namespace RazorSharp.Interop.Structures
{
	[Native]
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct ImageOptionalHeader64
	{
		public ushort Magic;


		public byte MajorLinkerVersion;


		public byte MinorLinkerVersion;


		public uint SizeOfCode;


		public uint SizeOfInitializedData;


		public uint SizeOfUninitializedData;


		public uint AddressOfEntryPoint;


		public uint BaseOfCode;


		public ulong ImageBase;


		public uint SectionAlignment;


		public uint FileAlignment;


		public ushort MajorOperatingSystemVersion;


		public ushort MinorOperatingSystemVersion;


		public ushort MajorImageVersion;


		public ushort MinorImageVersion;


		public ushort MajorSubsystemVersion;


		public ushort MinorSubsystemVersion;


		public uint Win32VersionValue;


		public uint SizeOfImage;


		public uint SizeOfHeaders;


		public uint CheckSum;


		public ushort Subsystem;


		public ushort DllCharacteristics;


		public ulong SizeOfStackReserve;


		public ulong SizeOfStackCommit;


		public ulong SizeOfHeapReserve;


		public ulong SizeOfHeapCommit;


		public uint LoaderFlags;


		public uint NumberOfRvaAndSizes;


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