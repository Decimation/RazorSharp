#region

using System;
using System.Runtime.InteropServices;
using System.Text;
using RazorCommon;
using RazorCommon.Diagnostics;
using RazorSharp.CoreClr.Enums.ObjHeader;

// ReSharper disable BuiltInTypeReferenceStyle
// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable InconsistentNaming

#endregion


namespace RazorSharp.CoreClr.Structures
{
	[StructLayout(LayoutKind.Explicit)]
	internal unsafe struct ObjHeader
	{
		#region Fields

#if WIN32
		[FieldOffset(0)] 
		private UInt32 m_uSyncBlockValue;
#else
		[FieldOffset(0)]
		private readonly UInt32 m_uAlignpad;

		[FieldOffset(4)]
		private UInt32 m_uSyncBlockValue;
#endif

		#endregion

		#region Accessors

		public UInt32         SyncBlock        => m_uSyncBlockValue;
		public SyncBlockFlags SyncBlockAsFlags => (SyncBlockFlags) m_uSyncBlockValue;

		#endregion

		static ObjHeader()
		{
			Conditions.Requires(sizeof(ObjHeader) == IntPtr.Size);
			Guard.UseRequires64Bit(nameof(ObjHeader));
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
		internal bool IsPinned()
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
			byte[] bytes = BitConverter.GetBytes(m_uSyncBlockValue);
			var    sb    = new StringBuilder();

			foreach (byte v in bytes) sb.AppendFormat("{0} ", Convert.ToString(v, 2));

			sb.Remove(sb.Length - 1, 1);
			return $"Sync block: {m_uSyncBlockValue} ({SyncBlockAsFlags}) [{bytes.AutoJoin()}] [{sb}]";
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
			return m_uSyncBlockValue == other.m_uSyncBlockValue;
		}

		public override int GetHashCode()
		{
			//return ((int) m_uAlignpad * 397) ^ (int) m_uSyncBlockValue;
			// ReSharper disable once NonReadonlyMemberInGetHashCode
			return m_uSyncBlockValue.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj?.GetType() == GetType()) {
				var h = (ObjHeader) obj;
				return Equals(h);
			}

			return false;
		}

		#endregion
	}
}