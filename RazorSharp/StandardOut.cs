using System;
using System.Text;
using RazorCommon;
using RazorInvoke;

namespace RazorSharp
{

	public static class StandardOut
	{
		public static void ModConsole()
		{
			System.Console.OutputEncoding = Encoding.Unicode; // todo: Encoding.Unicode / UTF8? Any difference?
			IntPtr handle = Kernel32.GetConsoleHandle();
			Kernel32.GetConsoleMode(handle, out uint mode);
			mode |= (uint) Enumerations.ConsoleOutputModes.EnableVirtualTerminalProcessing;
			Kernel32.SetConsoleMode(handle, mode);
			Logger.Log("Console modded");
		}
	}

}