#region

using System;
using System.Text;
using RazorSharp.Native;
using RazorSharp.Native.Enums;

#endregion

namespace RazorSharp.Utilities
{

	public static class StandardOut
	{
		/// <summary>
		///     Patch the console to allow for ANSI escape sequences and special formatting.
		/// </summary>
		public static void ModConsole()
		{
			Console.OutputEncoding = Encoding.Unicode; // todo: Encoding.Unicode / UTF8? Any difference?
			IntPtr handle = Kernel32.GetConsoleHandle();
			Kernel32.GetConsoleMode(handle, out ConsoleOutputModes mode);
			mode |= ConsoleOutputModes.EnableVirtualTerminalProcessing;
			Kernel32.SetConsoleMode(handle, mode);

//			Logger.Log(Level.Standard, Flags.Info, "Console modded");
		}
	}

}