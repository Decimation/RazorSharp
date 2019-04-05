#region

using System.Runtime.InteropServices;

// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable MemberCanBePrivate.Global

#endregion

namespace RazorSharp.Native.Structures.Images
{
	[StructLayout(LayoutKind.Sequential)]
	public struct ImageFileHeader
	{
		/// WORD->unsigned short
		public ushort Machine;

		/// WORD->unsigned short
		public ushort NumberOfSections;

		/// DWORD->unsigned int
		public uint TimeDateStamp;

		/// DWORD->unsigned int
		public uint PointerToSymbolTable;

		/// DWORD->unsigned int
		public uint NumberOfSymbols;

		/// WORD->unsigned short
		public ushort SizeOfOptionalHeader;

		/// WORD->unsigned short
		public ushort Characteristics;
	}
}