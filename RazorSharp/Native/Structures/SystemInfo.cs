#region

using System;
using System.Runtime.InteropServices;
using RazorSharp.Native.Enums;

#endregion

// ReSharper disable MemberCanBePrivate.Global

namespace RazorSharp.Native.Structures
{

	[StructLayout(LayoutKind.Sequential)]
	public struct SystemInfo
	{
		public readonly ProcessorArchitecture ProcessorArchitecture;
		public readonly ushort                Reserved;
		public readonly uint                  PageSize;
		public readonly IntPtr                MinimumApplicationAddress;
		public readonly IntPtr                MaximumApplicationAddress;
		public readonly IntPtr                ActiveProcessorMask;
		public readonly uint                  NumberOfProcessors;
		public readonly uint                  ProcessorType;
		public readonly uint                  AllocationGranularity;
		public readonly ushort                ProcessorLevel;
		public readonly ushort                ProcessorRevision;
	}

}