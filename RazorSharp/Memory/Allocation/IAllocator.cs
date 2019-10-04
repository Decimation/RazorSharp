using RazorSharp.Memory.Pointers;

namespace RazorSharp.Memory.Allocation
{
	/// <summary>
	/// Describes a type that contains memory allocation functions.
	/// </summary>
	public interface IAllocator
	{
		Pointer<byte> Alloc(int size);
		
		Pointer<byte> ReAlloc(Pointer<byte> p, int size);
		
		void Free(Pointer<byte> p);
	}
}