using RazorSharp.Pointers;

namespace RazorSharp.Memory
{
	public class Region
	{
		private readonly Pointer<byte> m_lo;
		private readonly Pointer<byte> m_hi;

		public long Size {
			get { return (long) (m_hi - m_lo); }
		}

		private Region(Pointer<byte> lo, Pointer<byte> hi)
		{
			m_lo = lo;
			m_hi = hi;
			
		}
		
		
	}
}