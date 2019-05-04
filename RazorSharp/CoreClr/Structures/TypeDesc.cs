using System;
using System.Runtime.InteropServices;

// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable BuiltInTypeReferenceStyle

namespace RazorSharp.CoreClr.Structures
{
	using DWORD = UInt32;
	
	[StructLayout(LayoutKind.Explicit)]
	internal struct TypeDesc
	{
		//
		// Low-order 8 bits of this flag are used to store the CorElementType, which
		// discriminates what kind of TypeDesc we are
		//
		// The remaining bits are available for flags
		//
		[FieldOffset(0)]
		private DWORD m_typeAndFlags;
	}
}