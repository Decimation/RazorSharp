#region

using System;
using System.Runtime.InteropServices;
using System.Text;
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

		private readonly int m_sectionNumber;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = IMAGE_SIZEOF_SHORT_NAME)]
		private readonly string m_sectionName;

		private readonly void* m_sectionAddress;
		private readonly int   m_sectionSize;

		private readonly ImageSectionHeader m_header;

		#endregion

		#region Accessors

		public int    SectionNumber  => m_sectionNumber;
		public IntPtr SectionAddress => (IntPtr) m_sectionAddress;
		public string SectionName    => m_sectionName;
		public int    SectionSize    => m_sectionSize;

		public IntPtr EndAddress =>
			PointerUtils.Add(m_sectionAddress, (byte*) m_sectionSize - 1).Address;

		public ImageSectionHeader SectionHeader => m_header;

		#endregion


		public ImageSectionInfo(int sectionNumber, string sectionName, void* sectionAddress, int sectionSize,
			ImageSectionHeader header)
		{
			m_sectionNumber  = sectionNumber;
			m_sectionName    = sectionName;
			m_sectionAddress = sectionAddress;
			m_sectionSize    = sectionSize;
			m_header         = header;
		}


		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat("Section #: {0}", m_sectionNumber).AppendLine();
			sb.AppendFormat("Name: {0}", m_sectionName).AppendLine();
			sb.AppendFormat("Address: {0:X}", SectionAddress.ToInt64()).AppendLine();
			sb.AppendFormat("End Address: {0:X}", EndAddress.ToInt64()).AppendLine();
			sb.AppendFormat("Size: {0}", m_sectionSize).AppendLine();

			return sb.ToString();
		}
	}

}