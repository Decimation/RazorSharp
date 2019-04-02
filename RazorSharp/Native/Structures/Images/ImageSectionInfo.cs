#region

using System;
using System.Runtime.InteropServices;
using System.Text;
using RazorCommon.Strings;
using RazorSharp.Memory;
using RazorSharp.Pointers;

#endregion

namespace RazorSharp.Native.Structures.Images
{
	/// <summary>
	///     Wraps an <see cref="ImageSectionHeader" />
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public unsafe struct ImageSectionInfo
	{
		internal const int IMAGE_SIZEOF_SHORT_NAME = 8;

		#region Fields

		private readonly void* m_sectionAddress;

		#endregion

		#region Accessors

		public int SectionNumber { get; }

		public Pointer<byte> SectionAddress => (IntPtr) m_sectionAddress;

		[field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = IMAGE_SIZEOF_SHORT_NAME)]
		public string SectionName { get; }

		public int SectionSize { get; }


		public Pointer<byte> EndAddress {
			get {
				// I don't know what's with the SectionSize - 1
				
				return SectionAddress + (byte*) SectionSize - 1;;
			}
		}


		public ImageSectionHeader SectionHeader { get; }

		#endregion


		public ImageSectionInfo(int                sectionNumber,
		                        string             sectionName,
		                        void*              sectionAddress,
		                        int                sectionSize,
		                        ImageSectionHeader header)
		{
			SectionNumber    = sectionNumber;
			SectionName      = sectionName;
			m_sectionAddress = sectionAddress;
			SectionSize      = sectionSize;
			SectionHeader    = header;
		}

		internal object[] Row =>
			new object[]
			{
				SectionNumber,
				SectionName,
				String.Format("{0} ({1} K)", SectionSize, SectionSize / Mem.BYTES_IN_KB),
				Hex.ToHex(SectionAddress.ToInt64()),
				Hex.ToHex(EndAddress.ToInt64()),
				SectionHeader.Characteristics,
				"-"
			};


		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.AppendFormat("Section #: {0}", SectionNumber).AppendLine();
			sb.AppendFormat("Name: {0}", SectionName).AppendLine();
			sb.AppendFormat("Address: {0:P}", SectionAddress).AppendLine();
			sb.AppendFormat("End Address: {0:P}", EndAddress).AppendLine();
			sb.AppendFormat("Size: {0}", SectionSize).AppendLine();

			return sb.ToString();
		}
	}
}