#region

using System.Runtime.InteropServices;

#endregion

namespace RazorSharp.Native.Structures.Images
{
	[StructLayout(LayoutKind.Sequential)]
	public struct ImageFileHeader
	{
		/// WORD->unsigned short
		public readonly ushort Machine;

		/// WORD->unsigned short
		public readonly ushort NumberOfSections;

		/// DWORD->unsigned int
		public readonly uint TimeDateStamp;

		/// DWORD->unsigned int
		public readonly uint PointerToSymbolTable;

		/// DWORD->unsigned int
		public readonly uint NumberOfSymbols;

		/// WORD->unsigned short
		public readonly ushort SizeOfOptionalHeader;

		/// WORD->unsigned short
		public readonly ushort Characteristics;
	}
}