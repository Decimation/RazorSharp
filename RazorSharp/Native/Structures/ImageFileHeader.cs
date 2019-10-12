using System;
using System.Runtime.InteropServices;

namespace RazorSharp.Native.Structures
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct ImageFileHeader
	{
		public UInt16 Machine;
		public UInt16 NumberOfSections;
		public UInt32 TimeDateStamp;
		public UInt32 PointerToSymbolTable;
		public UInt32 NumberOfSymbols;
		public UInt16 SizeOfOptionalHeader;
		public UInt16 Characteristics;
	}
}