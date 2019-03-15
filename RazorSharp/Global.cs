#region

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using RazorSharp.Memory;
using RazorSharp.Utilities;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

#endregion

namespace RazorSharp
{
	internal static class Global
	{
		internal const string CONTEXT_PROP = "Context";

		private const string OUTPUT_TEMPLATE =
			"[{Timestamp:HH:mm:ss} ({ThreadId}) {Level:u3}] [{Context}] {Message:lj}{NewLine}{Exception}";

		private const string OUTPUT_TEMPLATE_ALT =
			"[{Timestamp:HH:mm:ss.fff} <{ThreadId}> ({Context}) {Level:u3}] {Message}{NewLine}";

		internal static readonly Logger Log;

		// todo: make portable
		internal const string CLR_PDB_STR = @"C:\Users\Deci\Desktop\clrx.pdb";

		private const string CLR_SYM_NAME = "clr.pdb";

		internal static readonly FileInfo CLR_DLL;
		internal static readonly FileInfo CLR_PDB;

		static Global()
		{
			var levelSwitch = new LoggingLevelSwitch
			{
				MinimumLevel = LogEventLevel.Debug
			};

			Log = new LoggerConfiguration()
			     .Enrich.FromLogContext()
			     .Enrich.WithThreadId()
			     .MinimumLevel.ControlledBy(levelSwitch)
			     .WriteTo.Console(outputTemplate: OUTPUT_TEMPLATE_ALT, theme: SystemConsoleTheme.Colored)
			     .CreateLogger();

			CLR_DLL = GetClrDll();
			CLR_PDB = GetClrSymbolFile();
		}

		private static bool Search(DirectoryInfo dir, string name, out FileInfo fi)
		{
			var files = dir.GetFiles();

			foreach (var file in files) {
				if (file.Name.Contains(name)) {
					fi = file;
					return true;
				}
			}

			fi = null;
			return false;
		}


		private static Process Shell(string cmd)
		{
			var process = new Process();
			var startInfo = new ProcessStartInfo
			{
				WindowStyle = ProcessWindowStyle.Hidden,
				FileName    = "cmd.exe",
				Arguments   = "/C " + cmd
			};
			process.StartInfo = startInfo;
			process.Start();
			return process;
		}

		private static FileInfo GetClrDll()
		{
			string frameworkFolder = "Framework";

			var currentVer = Environment.Version;
			string ver = "v" + currentVer
			                  .ToString()
			                  .Replace(currentVer.Revision.ToString(), "");


			if (Mem.Is64Bit) {
				frameworkFolder += "64";
			}

			string dir = String.Format(@"C:\Windows\Microsoft.NET\{0}\{1}\clr.dll", frameworkFolder, ver);

			var clr = new FileInfo(dir);
			Conditions.RequiresFileExists(clr);
			return clr;
		}

		private static FileInfo DownloadClrSymbolFile()
		{
			// "c:\Program Files\Debugging Tools for Windows\symchk" c:\Windows\System32\d3dx9_30.dll /oc \.


			Log.Debug("Downloading symbols");
			var cd = new DirectoryInfo(Environment.CurrentDirectory);

			// "C:\Program Files (x86)\Windows Kits\10\Debuggers\x64\symchk" "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\clr.dll" /s SRV*c:\symbols*http://msdl.microsoft.com/download/symbols

			var symChk = new FileInfo(@"C:\Program Files (x86)\Windows Kits\10\Debuggers\x64\symchk");


			string cmd = String.Format("\"{0}\" \"{1}\" /s SRV*{2}*http://msdl.microsoft.com/download/symbols",
			                           symChk.FullName, CLR_DLL.FullName, cd.FullName);


			var cmdProc = Shell(cmd);


			var startTime = DateTimeOffset.Now;

			while (!cmdProc.HasExited) {
				if (DateTimeOffset.Now.Subtract(startTime).TotalMinutes > 1)
					throw new TimeoutException();
			}


			var pdb = new FileInfo(cd.FullName + @"\" + CLR_SYM_NAME);
			Conditions.RequiresFileExists(pdb);
			return pdb;
		}

		private static FileInfo FindSymbolFile(string dir)
		{
			var cd = new DirectoryInfo(dir);
			if (Search(cd, CLR_PDB_STR, out var file)) {
				return file;
			}

			return null;
		}

		internal static FileInfo GetClrSymbolFile()
		{
			FileInfo clrSym = null;

			string[] dirs = {Environment.CurrentDirectory, Environment.SystemDirectory};
			foreach (string dir in dirs) {
				var fi = FindSymbolFile(dir);
				if (fi != null) {
					clrSym = fi;
					break;
				}
			}

			if (clrSym == null) {
				clrSym = DownloadClrSymbolFile();
			}

			Log.Debug("Clr symbol file: {File}", clrSym.DirectoryName);

			return clrSym;
		}


		internal static void Setup()
		{
			Conditions.CheckCompatibility();


			//ClrFunctions.Init();
			Console.OutputEncoding = Encoding.Unicode;
		}

		internal static void Close()
		{
			if (Mem.IsMemoryInUse) {
				Log.Warning("Memory leak: {Count} dangling pointer(s)", Mem.AllocCount);
			}

			Log.Dispose();
		}
	}
}