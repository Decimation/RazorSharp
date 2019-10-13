using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace RazorSharp.Native.Structures
{
	[Native]
	[StructLayout(LayoutKind.Sequential)]
	public struct ImageDataDirectory
	{
		public uint VirtualAddress { get; }
		public uint Size { get; }
	}
}