using System.Runtime.InteropServices;

namespace RazorSharp.Native.Structures
{

	[StructLayout(LayoutKind.Sequential)]
	public struct IMAGEHLP_SYMBOL64
	{
		public uint  SizeOfStruct;  // set to sizeof(IMAGEHLP_SYMBOLW64)
		public ulong Address;       // virtual address including dll base address
		public uint  Size;          // estimated size of symbol, can be zero
		public uint  Flags;         // info about the symbols, see the SYMF defines
		public uint  MaxNameLength; // maximum size of symbol name in 'Name'
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 33)]
		public char[] Name; // symbol name (null terminated string)
	}

}