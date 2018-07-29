using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
// ReSharper disable InconsistentNaming

namespace RazorSharp.Runtime.CLRTypes
{
	[StructLayout(LayoutKind.Explicit)]
	public unsafe struct ObjHeader
	{

#if !WIN32
		[FieldOffset(0)]
		private readonly UInt32 m_uAlignpad;
#endif
		[FieldOffset(4)]
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

		public bool Equals(ObjHeader other)
		{
			return m_uAlignpad == other.m_uAlignpad && m_uSyncBlockValue == other.m_uSyncBlockValue;
		}

		public override int GetHashCode()
		{
			unchecked {
				return ((int) m_uAlignpad * 397) ^ (int) m_uSyncBlockValue;
			}
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