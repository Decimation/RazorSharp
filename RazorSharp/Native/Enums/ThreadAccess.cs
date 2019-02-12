#region

using System;

#endregion

namespace RazorSharp.Native.Enums
{
	[Flags]
	public enum ThreadAccess
	{
		None                    = 0,
		All                     = 0x1F03FF,
		DirectImpersonation     = 0x200,
		GetContext              = 0x008,
		Impersonate             = 0x100,
		QueryInformation        = 0x040,
		QueryLimitedInformation = 0x800,
		SetContext              = 0x010,
		SetInformation          = 0x020,
		SetLimitedInformation   = 0x400,
		SetThreadToken          = 0x080,
		SuspendResume           = 0x002,
		Terminate               = 0x001
	}
}