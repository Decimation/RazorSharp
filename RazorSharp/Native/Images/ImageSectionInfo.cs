#region

using System;
using System.Runtime.InteropServices;
using System.Text;
using SimpleSharp.Strings;
using RazorSharp.CoreClr;
using RazorSharp.Memory;
using RazorSharp.Memory.Pointers;

#endregion

namespace RazorSharp.Native.Images
{
	/// <summary>
	///     Wraps an <see cref="ImageSectionHeader" />
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public unsafe struct ImageSectionInfo
	{
		internal const int IMAGE_SIZEOF_SHORT_NAME = 8;

		#region Accessors

		public int SectionNumber { get; }

		public Pointer<byte> SectionAddress { get; }

		[field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = IMAGE_SIZEOF_SHORT_NAME)]
		public string SectionName { get; }

		public int SectionSize { get; }

		public Pointer<byte> EndAddress => SectionAddress + (byte*) SectionSize - 1;

		private ImageSectionHeader SectionHeader { get; }

		public ImageSectionCharacteristics Characteristics => SectionHeader.Characteristics;

		#endregion


		public static bool operator ==(ImageSectionInfo lhs, ImageSectionInfo rhs)
		{
			return lhs.Equals(rhs);
		}

		public static bool operator !=(ImageSectionInfo lhs, ImageSectionInfo rhs)
		{
			return !(lhs == rhs);
		}

		internal ImageSectionInfo(int                sectionNumber,
		                          string             sectionName,
		                          void*              sectionAddress,
		                          int                sectionSize,
		                          ImageSectionHeader header)
		{
			SectionNumber    = sectionNumber;
			SectionName      = sectionName;
			SectionAddress = sectionAddress;
			SectionSize      = sectionSize;
			SectionHeader    = header;
		}

		

		public bool Equals(ImageSectionInfo other)
		{
			return SectionAddress == other.SectionAddress && SectionNumber == other.SectionNumber &&
			       string.Equals(SectionName, other.SectionName) && SectionSize == other.SectionSize &&
			       SectionHeader.Equals(other.SectionHeader);
		}

		public override bool Equals(object obj)
		{
			return obj is ImageSectionInfo other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked {
				int hashCode = unchecked((int) (long) SectionAddress);
				hashCode = (hashCode * 397) ^ SectionNumber;
				hashCode = (hashCode * 397) ^ (SectionName != null ? SectionName.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ SectionSize;
				hashCode = (hashCode * 397) ^ SectionHeader.GetHashCode();
				return hashCode;
			}
		}


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