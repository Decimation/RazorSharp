using System.Diagnostics;
using RazorSharp.Memory.Pointers;

namespace RazorSharp.Memory
{
	public class Region
	{
		public Pointer<byte> BaseAddress { get; }
		public int Size { get; }

		public Region(Pointer<byte> p, int s)
		{
			BaseAddress = p;
			Size = s;
		}
		
		public Region(ProcessModule p) : this(p.BaseAddress, p.ModuleMemorySize) {}
	}
}