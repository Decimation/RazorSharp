using System;
using System.Diagnostics;

namespace RazorSharp.Runtime.CLRTypes
{

	public unsafe struct ObjHeader
	{
#if !WIN32
		private readonly UInt32 m_uAlignpad;
#endif
		private UInt32 m_uSyncBlockValue;

		public UInt32 Bits => m_uSyncBlockValue;

		public SyncBlockFlags BitsAsFlags => (SyncBlockFlags) m_uSyncBlockValue;

		static ObjHeader()
		{
			Debug.Assert(sizeof(ObjHeader) == IntPtr.Size);
		}

		public void SetBit(uint uBit)
		{
			// Should be interlocked

			m_uSyncBlockValue |= uBit;
		}

		public void ClearBit(uint uBit)
		{
			// Should be interlocked

			m_uSyncBlockValue &= ~uBit;
		}


		public void SetGCBit()
		{
			//m_uSyncBlockValue |= Constants.BIT_SBLK_GC_RESERVE;
			m_uSyncBlockValue |= (uint) SyncBlockFlags.BitSblkGcReserve;
		}


		public void ClearGCBit()
		{
			//m_uSyncBlockValue &= Constants.BIT_SBLK_GC_RESERVE;
			m_uSyncBlockValue &= (uint) SyncBlockFlags.BitSblkGcReserve;
		}

		public override string ToString()
		{
			return $"Sync block: {m_uSyncBlockValue} ({BitsAsFlags})";
		}

		public static bool operator ==(ObjHeader a, ObjHeader b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(ObjHeader a, ObjHeader b)
		{
			return !a.Equals(b);
		}

		public override bool Equals(object obj)
		{
			if (obj.GetType() == GetType()) {
				var h = (ObjHeader) obj;
				return h.m_uSyncBlockValue == m_uSyncBlockValue;
			}

			return false;
		}
	}

}