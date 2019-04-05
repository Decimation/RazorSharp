using System;
using System.Runtime.InteropServices;
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable MemberCanBePrivate.Global

namespace RazorSharp.Native.Structures.Images
{
	[StructLayout(LayoutKind.Sequential)]
	public struct ImageDataDirectory
	{
		public UInt32 VirtualAddress;
		public UInt32 Size;
	}
}