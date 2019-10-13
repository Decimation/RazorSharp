using System;
using System.Runtime.InteropServices;
using RazorSharp.Native.Enums;

namespace RazorSharp.Native.Structures
{
	[Native]
	[StructLayout(LayoutKind.Sequential)]
	internal struct MemoryBasicInformation
	{
		internal IntPtr           BaseAddress;
		internal IntPtr           AllocationBase;
		internal MemoryProtection AllocationProtect;
		internal IntPtr           RegionSize;
		internal MemState         State;
		internal MemoryProtection Protect;
		internal MemType          Type;
	}
}