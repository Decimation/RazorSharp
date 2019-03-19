using System.Collections;
using System.Collections.Generic;
using RazorSharp.Memory;

namespace RazorSharp.Pointers
{
	public struct AllocPointer<T> : IEnumerable<T>
	{
		private Pointer<T> m_ptr;

		public int Length {
			get { return AllocPool.GetLength(m_ptr); }
			set { this = AllocPool.ReAlloc(m_ptr, value); }
		}

		public int  Size        => AllocPool.GetSize(m_ptr);
		public bool IsAllocated => AllocPool.IsAllocated(m_ptr);

		public Pointer<T> Pointer {
			get { return m_ptr; }
//			set { m_ptr = value; }
		}

		public AllocPointer<T> Limit {
			get { return AllocPool.GetLimit(m_ptr); }
		}

		public AllocPointer<T> Origin {
			get { return AllocPool.GetOrigin(m_ptr); }
		}

		public int Offset {
			get { return AllocPool.GetOffset(m_ptr); }
		}


		public void Clear()
		{
			m_ptr.Zero(Length);
		}
		
		public void Free()
		{
//			Clear();
			AllocPool.Free(m_ptr);
		}
		
		public AllocPointer(Pointer<T> ptr)
		{
			m_ptr = ptr;
		}

		public static implicit operator AllocPointer<T>(Pointer<T> ptr)
		{
			return new AllocPointer<T>(ptr);
		}


		public T[] ToArray()
		{
			return m_ptr.CopyOut(Length);
		}

		public IEnumerator<T> GetEnumerator()
		{
			return m_ptr.GetEnumerator(Length);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public override string ToString()
		{

			return Pointer.ToTable(Length).ToMarkDownString();
		}
	}
}