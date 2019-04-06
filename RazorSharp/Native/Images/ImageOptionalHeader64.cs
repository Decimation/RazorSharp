#region

using System.Runtime.InteropServices;

// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable MemberCanBePrivate.Global

#endregion

namespace RazorSharp.Native.Images
{
	[StructLayout(LayoutKind.Explicit)]
	internal struct ImageOptionalHeader64
	{
		[FieldOffset(0)]
        public MagicType Magic;

        [FieldOffset(2)]
        public byte MajorLinkerVersion;

        [FieldOffset(3)]
        public byte MinorLinkerVersion;

        [FieldOffset(4)]
        public uint SizeOfCode;

        [FieldOffset(8)]
        public uint SizeOfInitializedData;

        [FieldOffset(12)]
        public uint SizeOfUninitializedData;

        [FieldOffset(16)]
        public uint AddressOfEntryPoint;

        [FieldOffset(20)]
        public uint BaseOfCode;

        [FieldOffset(24)]
        public ulong ImageBase;

        [FieldOffset(32)]
        public uint SectionAlignment;

        [FieldOffset(36)]
        public uint FileAlignment;

        [FieldOffset(40)]
        public ushort MajorOperatingSystemVersion;

        [FieldOffset(42)]
        public ushort MinorOperatingSystemVersion;

        [FieldOffset(44)]
        public ushort MajorImageVersion;

        [FieldOffset(46)]
        public ushort MinorImageVersion;

        [FieldOffset(48)]
        public ushort MajorSubsystemVersion;

        [FieldOffset(50)]
        public ushort MinorSubsystemVersion;

        [FieldOffset(52)]
        public uint Win32VersionValue;

        [FieldOffset(56)]
        public uint SizeOfImage;

        [FieldOffset(60)]
        public uint SizeOfHeaders;

        [FieldOffset(64)]
        public uint CheckSum;

        [FieldOffset(68)]
        public SubSystemType Subsystem;

        [FieldOffset(70)]
        public DllCharacteristics DllCharacteristics;

        [FieldOffset(72)]
        public ulong SizeOfStackReserve;

        [FieldOffset(80)]
        public ulong SizeOfStackCommit;

        [FieldOffset(88)]
        public ulong SizeOfHeapReserve;

        [FieldOffset(96)]
        public ulong SizeOfHeapCommit;

        [FieldOffset(104)]
        public uint LoaderFlags;

        [FieldOffset(108)]
        public uint NumberOfRvaAndSizes;

        [FieldOffset(112)]
        public ImageDataDirectory ExportTable;

        [FieldOffset(120)]
        public ImageDataDirectory ImportTable;

        [FieldOffset(128)]
        public ImageDataDirectory ResourceTable;

        [FieldOffset(136)]
        public ImageDataDirectory ExceptionTable;

        [FieldOffset(144)]
        public ImageDataDirectory CertificateTable;

        [FieldOffset(152)]
        public ImageDataDirectory BaseRelocationTable;

        [FieldOffset(160)]
        public ImageDataDirectory Debug;

        [FieldOffset(168)]
        public ImageDataDirectory Architecture;

        [FieldOffset(176)]
        public ImageDataDirectory GlobalPtr;

        [FieldOffset(184)]
        public ImageDataDirectory TLSTable;

        [FieldOffset(192)]
        public ImageDataDirectory LoadConfigTable;

        [FieldOffset(200)]
        public ImageDataDirectory BoundImport;

        [FieldOffset(208)]
        public ImageDataDirectory IAT;

        [FieldOffset(216)]
        public ImageDataDirectory DelayImportDescriptor;

        [FieldOffset(224)]
        public ImageDataDirectory CLRRuntimeHeader;

        [FieldOffset(232)]
        public ImageDataDirectory Reserved;
	}
}