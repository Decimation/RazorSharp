#region

using System;

#endregion

namespace RazorSharp.Native.Enums
{

	[Flags]
	public enum FreeTypes : uint
	{
		Decommit = 0x4000,
		Release  = 0x8000
	}

}