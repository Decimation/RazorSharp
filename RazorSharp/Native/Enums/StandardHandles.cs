#region

using System;

#endregion

namespace RazorSharp.Native.Enums
{
	[Flags]
	public enum StandardHandles
	{
		StdInputHandle  = -10,
		StdOutputHandle = -11,
		StdErrorHandle  = -12
	}
}