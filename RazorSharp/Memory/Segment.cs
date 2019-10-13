using System.Diagnostics;
using RazorSharp.Memory.Pointers;
using RazorSharp.Native.Structures;

namespace RazorSharp.Memory
{
	/// <summary>
	/// Represents a region in memory.
	/// </summary>
	public class Segment
	{
		public Pointer<byte> BaseAddress { get; }

		public int Size { get; }

		public Segment(Pointer<byte> p, int s)
		{
			BaseAddress = p;
			Size        = s;
		}

		public Segment(ImageSectionInfo p) : this(p.Address, p.Size) { }

		public Segment(ProcessModule p) : this(p.BaseAddress, p.ModuleMemorySize) { }
	}
}