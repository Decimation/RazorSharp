using System.Diagnostics;
using RazorCommon;
using RazorSharp.CoreClr.Structures;
using RazorSharp.Native.Structures;
using RazorSharp.Native.Structures.Images;
using RazorSharp.Pointers;

namespace RazorSharp.Memory
{
	public unsafe class AddressInfo
	{
		public AddressInfo(Pointer<byte> ptr)
		{
			Address = ptr;
			
			IsOnStack = Mem.IsOnStack(Address);
			IsInHeap = GCHeap.GlobalHeap.Reference.IsHeapPointer(Address.ToPointer());
			Page = ptr.Query();
			Module = Modules.FromAddress(ptr);
			Segment = Segments.GetSegment(ptr, Module?.FileName);
		}

		public Pointer<byte> Address { get; }

		public ImageSectionInfo Segment { get; }
		
		public ProcessModule Module { get; }
		
		public MemoryBasicInformation Page { get; }

		public bool IsOnStack { get; }

		public bool IsInHeap { get; }
	}
}