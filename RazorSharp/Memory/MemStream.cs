#region

using RazorSharp.Memory.Pointers;

#endregion

namespace RazorSharp.Memory
{
	public class MemStream
	{
		private readonly Pointer<byte> m_ptr;

		private int m_offset;

		public MemStream(Pointer<byte> ptr)
		{
			m_ptr = ptr;
		}

		public T Read<T>()
		{
			int size = Unsafe.SizeOf<T>();
			var val  = m_ptr.ReadAny<T>(m_offset / size);

			m_offset += size;
			return val;
		}
	}
}