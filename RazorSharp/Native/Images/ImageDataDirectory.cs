using System;
using System.Runtime.InteropServices;

// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable MemberCanBePrivate.Global

namespace RazorSharp.Native.Images
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct ImageDataDirectory
	{
		public UInt32 VirtualAddress;
		public UInt32 Size;
	}
}