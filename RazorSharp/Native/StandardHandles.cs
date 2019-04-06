#region

using System;

#endregion

namespace RazorSharp.Native
{
	[Flags]
	internal enum StandardHandles
	{
		StdInputHandle  = -10,
		StdOutputHandle = -11,
		StdErrorHandle  = -12
	}
}