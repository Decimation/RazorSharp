using System;
using System.Runtime.InteropServices;

namespace RazorSharp.Native.Structures
{
	[StructLayout(LayoutKind.Sequential)]
	public struct ImageDataDirectory
	{
		public UInt32 VirtualAddress;
		public UInt32 Size;
	}
}