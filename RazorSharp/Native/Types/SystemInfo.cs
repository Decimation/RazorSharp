#region

using System;
using System.Runtime.InteropServices;
using RazorSharp.Native.Enums;

// ReSharper disable FieldCanBeMadeReadOnly.Global

#endregion

// ReSharper disable MemberCanBePrivate.Global

namespace RazorSharp.Native.Structures
{
	[StructLayout(LayoutKind.Sequential)]
	public struct SystemInfo
	{
		public ProcessorArchitecture ProcessorArchitecture;
		public ushort                Reserved;
		public uint                  PageSize;
		public IntPtr                MinimumApplicationAddress;
		public IntPtr                MaximumApplicationAddress;
		public IntPtr                ActiveProcessorMask;
		public uint                  NumberOfProcessors;
		public uint                  ProcessorType;
		public uint                  AllocationGranularity;
		public ushort                ProcessorLevel;
		public ushort                ProcessorRevision;
	}
}