using System;

namespace RazorSharp.Native
{
	[Flags]
	public enum ProcessHeapEntryFlags : ushort
	{
		PROCESS_HEAP_ENTRY_BUSY = 0x004,
		
		PROCESS_HEAP_ENTRY_DDESHARE = 0x0020,
		
		PROCESS_HEAP_ENTRY_MOVEABLE = 0x0010,
		
		PROCESS_HEAP_REGION = 0x0001,
		
		PROCESS_HEAP_UNCOMMITTED_RANGE = 0x0002,
	}
}