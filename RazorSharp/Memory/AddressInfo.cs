#region

using System.Diagnostics;
using RazorSharp.CoreClr.Structures;
using RazorSharp.Memory.Pointers;
using RazorSharp.Native;
using RazorSharp.Native.Images;
using RazorSharp.Native.Win32;

// ReSharper disable UnusedAutoPropertyAccessor.Global

#endregion

namespace RazorSharp.Memory
{
	public unsafe class AddressInfo
	{
		public AddressInfo(Pointer<byte> ptr)
		{
			Address = ptr;

			IsOnStack = Mem.IsOnStack(Address);
			IsInHeap  = GCHeap.GlobalHeap.Reference.IsHeapPointer(Address.ToPointer());
			Page      = Kernel32.VirtualQuery(ptr.Address);
			Module    = Modules.FromAddress(ptr);
			Segment   = Segments.GetSegment(ptr, Module?.FileName);
		}

		public Pointer<byte> Address { get; }

		public ImageSectionInfo Segment { get; }

		public ProcessModule Module { get; }

		public MemoryBasicInformation Page { get; }

		public bool IsOnStack { get; }

		public bool IsInHeap { get; }
	}
}