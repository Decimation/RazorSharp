using System;

// ReSharper disable InconsistentNaming

namespace RazorSharp.Native.Structures
{
	[Native]
	public struct ImageDOSHeader
	{
		// DOS .EXE header

		/// <summary>
		/// Magic number
		/// </summary>
		public ushort EMagic { get; }

		/// <summary>
		/// Bytes on last page of file
		/// </summary>
		public ushort ECblp { get; }

		/// <summary>
		/// Pages in file
		/// </summary>
		public ushort ECp { get; }

		/// <summary>
		/// Relocations
		/// </summary>
		public ushort ECrlc { get; }

		/// <summary>
		/// Size of header in paragraphs
		/// </summary>
		public ushort ECparhdr { get; }


		/// <summary>
		/// Minimum extra paragraphs needed
		/// </summary>
		public ushort EMinalloc { get; }

		/// <summary>
		/// Maximum extra paragraphs needed
		/// </summary>
		public ushort EMaxalloc { get; }

		/// <summary>
		/// Initial (relative) SS value
		/// </summary>
		public ushort ESs { get; }

		/// <summary>
		/// Initial SP value
		/// </summary>
		public ushort ESp { get; }

		/// <summary>
		/// Checksum
		/// </summary>
		public ushort ECsum { get; }

		/// <summary>
		/// Initial IP value
		/// </summary>
		public ushort EIp { get; }

		/// <summary>
		/// Initial (relative) CS value
		/// </summary>
		public ushort ECs { get; }

		/// <summary>
		/// File address of relocation table
		/// </summary>
		public ushort ELfarlc { get; }

		/// <summary>
		/// Overlay number
		/// </summary>
		public ushort EOvno { get; }

		/// <summary>
		/// Reserved
		/// </summary>
		public ushort ERes0 { get; }

		/// <summary>
		/// Reserved
		/// </summary>
		public ushort ERes1 { get; }

		/// <summary>
		/// Reserved
		/// </summary>
		public ushort ERes2 { get; }

		/// <summary>
		/// Reserved
		/// </summary>
		public ushort ERes3 { get; }

		/// <summary>
		/// OEM identifier (for <see cref="EOeminfo"/>)
		/// </summary>
		public ushort EOemid { get; }

		/// <summary>
		/// OEM information; <see cref="EOemid"/> specific
		/// </summary>
		public ushort EOeminfo { get; }

		/// <summary>
		/// Reserved
		/// </summary>
		public ushort ERes20 { get; }

		/// <summary>
		/// Reserved
		/// </summary>
		public ushort ERes21 { get; }

		/// <summary>
		/// Reserved
		/// </summary>
		public ushort ERes22 { get; }

		/// <summary>
		/// Reserved
		/// </summary>
		public ushort ERes23 { get; }

		/// <summary>
		/// Reserved
		/// </summary>
		public ushort ERes24 { get; }

		/// <summary>
		/// Reserved
		/// </summary>
		public ushort ERes25 { get; }

		/// <summary>
		/// Reserved
		/// </summary>
		public ushort ERes26 { get; }

		/// <summary>
		/// Reserved
		/// </summary>
		public ushort ERes27 { get; }

		/// <summary>
		/// Reserved
		/// </summary>
		public ushort ERes28 { get; }

		/// <summary>
		/// Reserved
		/// </summary>
		public ushort ERes29 { get; }

		/// <summary>
		/// File address of new exe header
		/// </summary>
		public uint ELfanew { get; }
	}
}