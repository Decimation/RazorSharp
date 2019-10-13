using System.Runtime.InteropServices;
using RazorSharp.Interop.Utilities;

namespace RazorSharp.Interop.Structures
{
	[Native]
	[StructLayout(LayoutKind.Sequential)]
	public struct ImageDataDirectory
	{
		public uint VirtualAddress { get; }
		public uint Size { get; }
	}
}