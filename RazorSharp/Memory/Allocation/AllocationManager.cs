using System;
using System.Collections.Generic;
using RazorSharp.Memory.Pointers;
using RazorSharp.Model;
using SimpleSharp.Diagnostics;

namespace RazorSharp.Memory.Allocation
{
	/// <summary>
	/// Allocates and manages memory/resources.
	/// </summary>
	public class AllocationManager : Closable
	{
		/// <summary>
		///     Counts the number of allocations (allocated pointers)
		/// </summary>
		public int AllocCount => m_pointers.Count;
		
		public bool IsMemoryInUse => AllocCount > default(int);

		private readonly List<Pointer<byte>> m_pointers;
		
		private readonly IAllocator m_allocator;
		
		protected override string Id => nameof(AllocationManager);

		public AllocationManager(IAllocator allocator)
		{
			m_allocator = allocator;
			m_pointers = new List<Pointer<byte>>();
		}

		/// <summary>
		///     <para>
		///         Allocates <paramref name="elemCnt" /> elements of type <typeparamref name="T" /> in zeroed memory
		///         using the specified allocate function.
		///     </para>
		///     
		///     <para>
		///         Once you are done using the memory, dispose using <see cref="Free{T}(Pointer{T})" />
		///     </para>
		/// </summary>
		/// <typeparam name="T">Element type to allocate</typeparam>
		/// <returns>A pointer to the allocated memory</returns>
		public Pointer<T> Alloc<T>(int elemCnt = 1)
		{
			Conditions.Require(elemCnt > default(int), nameof(elemCnt));
			int size  = Mem.FullSize<T>(elemCnt);
			var alloc = m_allocator.Alloc(size);
			
			alloc.Clear();

			m_pointers.Add(alloc);

			return alloc;
		}

		public Pointer<T> ReAlloc<T>(Pointer<T> p, int elemCnt = 1)
		{
			int size = Mem.FullSize<T>(elemCnt);
			var reAlloc = m_allocator.ReAlloc(p.Address, size);

			var i = m_pointers.IndexOf(p.Cast());
			m_pointers[i] = reAlloc;
			
			return reAlloc;
		}

		public void Free<T>(Pointer<T> p)
		{
			m_allocator.Free(p.Cast());
			m_pointers.Remove(p.Cast());
		}


		public override string ToString()
		{
			return String.Format("Number of allocations: {0}", AllocCount);
		}

		public override void Close()
		{
			foreach (Pointer<byte> pointer in m_pointers) {
				Free(pointer);
			}
			
			base.Close();
		}
	}
}