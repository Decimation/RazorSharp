#region

using System;

#endregion

namespace RazorSharp.CoreClr.Structures.Enums
{
	/// <summary>
	///     <remarks>
	///         Use with <see cref="ObjHeader.SyncBlock" />
	///     </remarks>
	/// </summary>
	[Flags]
	public enum SyncBlockFlags : uint
	{
		BitSblkStringHasNoHighChars = 0x80000000,
		BitSblkAgileInProgress      = 0x80000000,
		BitSblkStringHighCharsKnown = 0x40000000,
		BitSblkStringHasSpecialSort = 0xC0000000,
		BitSblkStringHighCharMask   = 0xC0000000,
		BitSblkFinalizerRun         = 0x40000000,
		BitSblkGcReserve            = 0x20000000,
		BitSblkSpinLock             = 0x10000000,
		BitSblkIsHashOrSyncblkindex = 0x08000000,
		BitSblkIsHashcode           = 0x04000000
	}
}