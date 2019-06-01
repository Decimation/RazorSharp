#region

using System;
using System.Runtime.InteropServices;
using RazorSharp.Native.Win32.Enums;

// ReSharper disable FieldCanBeMadeReadOnly.Global

#endregion

// ReSharper disable MemberCanBePrivate.Global

namespace RazorSharp.Native.Win32.Structures
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct SystemInfo
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