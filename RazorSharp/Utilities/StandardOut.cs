#region

#region

using System;
using System.Text;
using RazorSharp.Native;
using RazorSharp.Native.Win32;
using RazorSharp.Native.Win32.Enums;

#endregion

#pragma warning disable 162

#endregion

namespace RazorSharp.Utilities
{
	internal static class StandardOut
	{
		/// <summary>
		///     Patch the console to allow for ANSI escape sequences and special formatting.
		/// </summary>
		[Obsolete]
		internal static void ModConsole()
		{
			Console.OutputEncoding = Encoding.Unicode; // todo: Encoding.Unicode / UTF8? Any difference?
			var handle = Kernel32.GetConsoleHandle();
			Kernel32.GetConsoleMode(handle, out var mode);
			mode |= ConsoleOutputModes.EnableVirtualTerminalProcessing;
			Kernel32.SetConsoleMode(handle, mode);

//			Logger.Log(Level.Standard, Flags.Info, "Console modded");
		}
	}
}