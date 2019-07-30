using System;
using System.Collections.Generic;
using RazorSharp.Memory;
using RazorSharp.Memory.Pointers;
using SimpleSharp.Diagnostics;

namespace RazorSharp.Model
{
	/// <summary>
	/// Describes a type that allocates memory or resources.
	/// </summary>
	public class Allocator : IClosable
	{
		/// <summary>
		///     Counts the number of allocations (allocated pointers)
		/// </summary>
		public int AllocCount => m_pointers.Count;
		
		public bool IsMemoryInUse => AllocCount > 0;

		private readonly List<Pointer<byte>> m_pointers;
		
		public delegate IntPtr AllocFunction(int size);

		public delegate IntPtr ReAllocFunction(IntPtr p, int size);

		public delegate void FreeFunction(IntPtr p);

		private readonly AllocFunction m_alloc;

		private readonly ReAllocFunction m_reAlloc;

		private readonly FreeFunction m_free;

		public Allocator(AllocFunction alloc, ReAllocFunction reAlloc, FreeFunction free)
		{
			m_alloc   = alloc;
			m_reAlloc = reAlloc;
			m_free    = free;
			m_pointers = new List<Pointer<byte>>();
		}

		/// <summary>
		///     <para>
		///         Allocates <paramref name="elemCnt" /> elements of type <typeparamref name="T" /> in zeroed, unmanaged memory
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
			Conditions.Require(elemCnt > 0, nameof(elemCnt));
			int size  = Mem.FullSize<T>(elemCnt);
			var alloc = m_alloc(size);
			
			((Pointer<byte>) alloc).Clear();

			m_pointers.Add(alloc);

			return alloc;
		}

		public Pointer<T> ReAlloc<T>(Pointer<T> p, int elemCnt = 1)
		{
			int size = Mem.FullSize<T>(elemCnt);
			var reAlloc = m_reAlloc(p.Address, size);

			var i = m_pointers.IndexOf(p.Cast());
			m_pointers[i] = reAlloc;
			
			return reAlloc;
		}

		public void Free<T>(Pointer<T> p)
		{
			m_free(p.Address);
			m_pointers.Remove(p.Cast());
		}


		public override string ToString()
		{
			return String.Format("Number of allocations: {0}", AllocCount);
		}

		public void Close()
		{
			foreach (Pointer<byte> pointer in m_pointers) {
				Free(pointer);
			}
		}
	}
}