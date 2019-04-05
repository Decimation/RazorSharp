#region

using System;

#endregion

namespace RazorSharp.Native.Enums
{
	[Flags]
	public enum ConsoleOutputModes : uint
	{
		EnableProcessedOutput           = 0x0001,
		EnableWrapAtEolOutput           = 0x0002,
		EnableVirtualTerminalProcessing = 0x0004,
		DisableNewlineAutoReturn        = 0x0008,
		EnableLvbGridWorldwide          = 0x0010
	}
}