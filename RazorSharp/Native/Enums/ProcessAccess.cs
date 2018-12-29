#region

using System;

#endregion

namespace RazorSharp.Native.Enums
{

	// todo: compare to ProcessAccessFlags

	[Flags]
	public enum ProcessAccess
	{
		All                     = 0x1F0FFF,
		Terminate               = 0x000001,
		CreateThread            = 0x000002,
		VmRead                  = 0x000010,
		VmWrite                 = 0x000020,
		CreateProcess           = 0x000080,
		QueryInformation        = 0x000400,
		QueryLimitedInformation = 0x001000

	}

}