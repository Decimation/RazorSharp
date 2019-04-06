using System.Runtime.InteropServices;
using RazorSharp.Native.Symbols;

// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable MemberCanBePrivate.Global

namespace RazorSharp.Native.Images
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct ImageHelpModule64
	{
		//************************************************
		public int     SizeOfStruct;
		public long    BaseOfImage;
		public int     ImageSize;
		public int     TimeDateStamp;
		public int     CheckSum;
		public int     NumSyms;
		public SymType SymType;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		public string ModuleName;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		public string ImageName;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		public string LoadedImageName;

		//************************************************
		//new elements v2
		//*************************************************
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		public string LoadedPdbName;

		public int CVSig;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 780)]
		public string CVData;

		public int  PdbSig;
		public GUID PdbSig70;
		public int  PdbAge;

		[MarshalAs(UnmanagedType.Bool)]
		public bool PdbUnmatched;

		[MarshalAs(UnmanagedType.Bool)]
		public bool DbgUnmatched;

		[MarshalAs(UnmanagedType.Bool)]
		public bool LineNumbers;

		[MarshalAs(UnmanagedType.Bool)]
		public bool GlobalSymbols;

		[MarshalAs(UnmanagedType.Bool)]
		public bool TypeInfo;

		//************************************************
		//new elements v3
		//************************************************
		[MarshalAs(UnmanagedType.Bool)]
		public bool SourceIndexed;

		[MarshalAs(UnmanagedType.Bool)]
		public bool Publics;

		//************************************************
		//new elements v4
		//************************************************
		public int MachineType;

		public int Reserved;
		//************************************************
	}
}