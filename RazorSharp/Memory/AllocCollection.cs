using System;
using System.Collections;
using System.Collections.Generic;
using RazorSharp.Pointers;

namespace RazorSharp.Memory
{
	// todo: WIP
	public class AllocCollection<T> : IDisposable, IEnumerable<T>
	{
		private          Pointer<T> m_pAlloc;
		private          int        m_cb;
		private readonly int        m_elemSize;

		public int Size => m_cb;

		public int Count {
			get { return m_cb / m_elemSize; }
		}

		private Pointer<T> AllocEnd {
			get { return m_pAlloc + m_cb; }
		}

		public int ElementSize => m_elemSize;

		public ref T this[int index] => ref m_pAlloc[index];


		public AllocCollection(int elemCnt)
		{
			m_elemSize = Unsafe.SizeOf<T>();
			m_cb       = m_elemSize * elemCnt;
			m_pAlloc   = Mem.AllocUnmanaged<T>(elemCnt);
		}

		public void Dispose()
		{
			Zero();
			Mem.Free(m_pAlloc.Address);
		}

		private void Zero()
		{
			Mem.Zero(m_pAlloc.Address, m_cb);
		}


		public IEnumerator<T> GetEnumerator()
		{
			for (int i = 0; i < Count; i++) {
				yield return this[i];
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

}