#region

using System;
using System.Runtime.InteropServices;
using System.Text;
using RazorCommon;
using RazorSharp.Utilities;

// ReSharper disable BuiltInTypeReferenceStyle
// ReSharper disable ConvertToAutoPropertyWhenPossible

#endregion

// ReSharper disable InconsistentNaming

namespace RazorSharp.CLR.Structures
{

	[StructLayout(LayoutKind.Explicit)]
	public unsafe struct ObjHeader
	{

		#region Fields

#if !WIN32
		[FieldOffset(0)] private readonly UInt32 m_uAlignpad;
#endif
		[FieldOffset(4)] private UInt32 m_uSyncBlockValue;

		#endregion


		#region Accessors

		public UInt32         SyncBlock        => m_uSyncBlockValue;
		public SyncBlockFlags SyncBlockAsFlags => (SyncBlockFlags) m_uSyncBlockValue;

		#endregion


		static ObjHeader()
		{
			RazorContract.Assert(sizeof(ObjHeader) == IntPtr.Size);
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

		// todo
		public bool IsPinned()
		{
			//BIT_SBLK_GC_RESERVE
			//return !!((((CObjectHeader*)this)->GetHeader()->GetBits()) & BIT_SBLK_GC_RESERVE);
			return !!((SyncBlock & (uint) SyncBlockFlags.BitSblkGcReserve) != 0);
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
			byte[]        bytes = BitConverter.GetBytes(m_uSyncBlockValue);
			StringBuilder sb    = new StringBuilder();

			foreach (byte v in bytes) {
				sb.AppendFormat("{0} ", Convert.ToString(v, 2));
			}

			sb.Remove(sb.Length - 1, 1);
			return $"Sync block: {m_uSyncBlockValue} ({SyncBlockAsFlags}) [{Collections.ToString(bytes)}] [{sb}]";
		}

		#region Equality

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
			if (obj?.GetType() == GetType()) {
				ObjHeader h = (ObjHeader) obj;
				return Equals(h);
			}

			return false;
		}

		#endregion


	}

}