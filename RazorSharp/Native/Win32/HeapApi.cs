using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace RazorSharp.Native.Win32
{
	internal static class HeapApi
	{
		[DllImport(Kernel32.KERNEL32_DLL)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool HeapWalk(IntPtr hHeap, IntPtr lpEntry);
		
		[DllImport(Kernel32.KERNEL32_DLL)]
		internal static extern IntPtr GetProcessHeap();
		
		[DllImport(Kernel32.KERNEL32_DLL)]
		internal static extern uint GetProcessHeaps(uint numberOfHeaps, IntPtr processHeaps);

		[DllImport(Kernel32.KERNEL32_DLL)]
		internal static extern IntPtr HeapAlloc(IntPtr hHeap, uint dwFlags, uint dwBytes);
		
		

		internal static unsafe void DebugHeaps()
		{
			ulong totalBytes       = 0;
			ulong committedBytes   = 0;
			ulong uncommittedBytes = 0;
			bool  bFirstBlock      = false;

			var heapEntries = GetHeapEntries();

			var hPrivate      = GetProcessHeap();
			var gcHandle      = GCHandle.Alloc(heapEntries, GCHandleType.Pinned);
			var iterHeapEntry = (ProcessHeapEntry*) gcHandle.AddrOfPinnedObject();

			for (int pos = 0; pos < heapEntries.Length; pos++) {
				var tempHeapEntry = *iterHeapEntry;
				// This represents the REGION block,
				// which will containing all the details about
				// committed block and uncommitted block present in a Heap.

				if (!bFirstBlock) {
					totalBytes = (ulong) tempHeapEntry.lpData -
					             (ulong) hPrivate;
					totalBytes += (ulong) tempHeapEntry.region.lpFirstBlock -
					              (ulong) tempHeapEntry.lpData;
					committedBytes += (ulong) tempHeapEntry.region.lpFirstBlock -
					                  (ulong) tempHeapEntry.lpData;
					committedBytes += (ulong) tempHeapEntry.lpData -
					                  (ulong) hPrivate;
					bFirstBlock = true;
				}

				// This represents the allocated blocks in a heap.
				// The amount of bytes in the block is obtained 
				// by subtracting the starting address [virtual address]
				// of the next block from the 
				// starting address [ virtual address ] of the present block.
				if (tempHeapEntry.wFlags == (ushort) ProcessHeapEntryFlags.PROCESS_HEAP_ENTRY_BUSY) {
					ulong bytesAllocated =
						(ulong) heapEntries[pos + 1].lpData -
						(ulong) tempHeapEntry.lpData;
					totalBytes     += bytesAllocated;
					committedBytes += bytesAllocated;
				}

				// This represents the committed block which
				// is free, i.e. not being allocated or not being used
				// as control structure. Data member cbData represents
				// the size in bytes for this range of free block.

				if (tempHeapEntry.wFlags == 0) {
					totalBytes     += tempHeapEntry.cbData;
					committedBytes += tempHeapEntry.cbData;
				}

				// For Uncommitted block, cbData represents the size
				// (in bytes) for range of uncommitted memory.
				//
				if (tempHeapEntry.wFlags == (ushort) ProcessHeapEntryFlags.PROCESS_HEAP_UNCOMMITTED_RANGE) {
					uncommittedBytes += tempHeapEntry.cbData;
					totalBytes       += tempHeapEntry.cbData;
				}
			}

			Global.Log.Debug("Total bytes in heap {Count}", totalBytes);
			Global.Log.Debug("Total committed bytes in heap {Count}", committedBytes);
			Global.Log.Debug("Total uncommitted bytes in heap {Count}", uncommittedBytes);

			gcHandle.Free();
		}

		internal static IntPtr[] GetProcessHeaps()
		{
			const int NUM_HANDLES = 256;
			var rgHandles = new IntPtr[NUM_HANDLES];
			var gcHandle = GCHandle.Alloc(rgHandles, GCHandleType.Pinned);

			var numHandles = (int) GetProcessHeaps(NUM_HANDLES, gcHandle.AddrOfPinnedObject());
			
			Array.Resize(ref rgHandles, numHandles);

			gcHandle.Free();
			
			return rgHandles;
		}
		
		internal static unsafe ProcessHeapEntry[] GetHeapEntries()
		{
			var list     = new List<ProcessHeapEntry>();
			var hHeap    = GetProcessHeap();
			var procHeap = new ProcessHeapEntry();
			
			while (HeapWalk(hHeap, new IntPtr(&procHeap))) {
				list.Add(procHeap);
			}

			return list.ToArray();
		}
	}
}