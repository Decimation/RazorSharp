using System;
using System.Diagnostics;
using RazorSharp.CLR.Structures;
using RazorSharp.Native;
using RazorSharp.Pointers;

namespace RazorSharp.Memory
{
	public class MemScanner
	{
		public delegate byte[] ReadMemoryFunction(int size);

		public enum MemoryRegion
		{
			Stack,
			Heap
		}

		private readonly Pointer<byte> m_addr;
		private readonly MemoryRegion  m_region;
		private          byte[]        m_buf;

		public MemScanner(MemoryRegion region, bool autoFillBuffer = true)
		{
			m_region = region;

			switch (m_region) {
				case MemoryRegion.Stack:
					m_addr = Mem.StackBase;

					break;
				case MemoryRegion.Heap:
					m_addr = GCHeap.LowestAddress;

					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			if (autoFillBuffer)
				UpdateBuffer();
		}

		public byte[] ReadMemory(int size)
		{
			return m_addr.CopyOut(size);
		}

		public byte[] KernelReadMemory(int size)
		{
			return Kernel32.ReadCurrentProcessMemory(m_addr, size);
		}

		public byte[] SafeReadMemory(int size)
		{
			return m_addr.SafeCopyOut(size);
		}

		public void UpdateBuffer()
		{
			UpdateBuffer(ReadMemory);
		}

		public void UpdateBuffer(ReadMemoryFunction fn)
		{
			int bufSize = 0;
			switch (m_region) {
				case MemoryRegion.Stack:
					bufSize = (int) Mem.StackSize;
					break;
				case MemoryRegion.Heap:
					bufSize = (int) GCHeap.Size;

					break;
				default:
					throw new ArgumentOutOfRangeException();
			}


			m_buf = fn(bufSize);
			Global.Log.Information("{BufLen} {BufSize}", m_buf.Length, bufSize);
			Debug.Assert(m_buf.Length == bufSize);
		}
	}
}