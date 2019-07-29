using System;
using System.Runtime.InteropServices;
using RazorSharp.Memory;

namespace RazorSharp.CoreClr.Metadata
{
	[Flags]
	public enum SyncBlockFlags : uint
	{
		StringHasNoHighChars = 0x80000000,
		AgileInProgress      = 0x80000000,
		StringHighCharsKnown = 0x40000000,
		StringHasSpecialSort = 0xC0000000,
		StringHighCharMask   = 0xC0000000,
		FinalizerRun         = 0x40000000,
		GcReserve            = 0x20000000,
		SpinLock             = 0x10000000,
		IsHashOrSyncblkindex = 0x08000000,
		IsHashcode           = 0x04000000
	}
	
	[StructLayout(LayoutKind.Explicit)]
	public struct ObjHeader
	{
		#region Fields

		/// <summary>
		/// This is the sync block on x86. This is padding on x64.
		/// </summary>
		[FieldOffset(default)]
		private readonly int m_dword1;

		/// <summary>
		/// This is the sync block on x64.
		/// </summary>
		[FieldOffset(sizeof(int))]
		private readonly int m_dword2;

		#endregion

		private int SyncBlockValue => !Mem.Is64Bit ? m_dword1 : m_dword2;

		public SyncBlockFlags Flags => (SyncBlockFlags) SyncBlockValue;


		public override string ToString()
		{
			return String.Format("Sync block: {0}", Flags);
		}
	}
}