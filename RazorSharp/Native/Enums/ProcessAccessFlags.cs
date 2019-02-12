#region

using System;

#endregion

namespace RazorSharp.Native.Enums
{
	[Flags]
	public enum ProcessAccessFlags : uint
	{
		All              = 0x001F0FFF,
		Terminate        = 0x00000001,
		CreateThread     = 0x00000002,
		VmOperation      = 0x00000008,
		VmRead           = 0x00000010,
		VmWrite          = 0x00000020,
		DupHandle        = 0x00000040,
		SetInformation   = 0x00000200,
		QueryInformation = 0x00000400,
		Synchronize      = 0x00100000
	}
}