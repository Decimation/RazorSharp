using System;
using System.Runtime.InteropServices;

// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable BuiltInTypeReferenceStyle

namespace RazorSharp.Native
{
	using DWORD = UInt32;
	using WORD = UInt16;
	
	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct Block
	{
		public       IntPtr hMem;
		public fixed UInt32  dwReserved[3];
	}
}