#region

using System.Runtime.InteropServices;

// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable MemberCanBePrivate.Global

#endregion

namespace RazorSharp.Native.Images
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct ImageFileHeader
	{
		/// WORD->unsigned short
		public MachineType Machine;

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