#region

using System;

#endregion

namespace RazorSharp.Native
{
	[Flags]
	internal enum FreeTypes : uint
	{
		Decommit = 0x4000,
		Release  = 0x8000
	}
}