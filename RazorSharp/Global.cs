#region

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using RazorCommon.Diagnostics;
using RazorCommon.Extensions;
using RazorCommon.Utilities;
using RazorSharp.CoreClr;
using RazorSharp.Diagnostics;
using RazorSharp.Memory;
using RazorSharp.Utilities;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

#endregion

namespace RazorSharp
{
	
	public static class Global
	{
		internal const string CONTEXT_PROP = "Context";

		private const string OUTPUT_TEMPLATE =
			"[{Timestamp:HH:mm:ss} ({ThreadId}) {Level:u3}] [{Context}] {Message:lj}{NewLine}{Exception}";

		private const string OUTPUT_TEMPLATE_ALT =
			"[{Timestamp:HH:mm:ss.fff} <{ThreadId}> ({Context}) {Level:u3}] {Message}{NewLine}";

		internal static readonly Logger Log;
		
		public static bool IsSetup { get; private set; }
		
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

			IsSetup = true;
		}

		private static void CheckCompatibility()
		{
			/**
			 * RazorSharp
			 *
			 * History:
			 * 	- RazorSharp (deci-common-c)
			 * 	- RazorSharpNeue
			 * 	- RazorCLR
			 * 	- RazorSharp
			 *
			 * Notes:
			 *  - 32-bit is not fully supported
			 *  - Most types are probably not thread-safe
			 *
			 * Goals:
			 *  - Provide identical and better functionality of ClrMD, SOS, and Reflection
			 * 	  but in a faster and more efficient way
			 */

			/**
			 * RazorSharp is tested on and targets:
			 *
			 * - x64
			 * - Windows
			 * - .NET CLR 4.7.2
			 * - Workstation Concurrent GC
			 *
			 */
			Conditions.Requires64Bit();
			Conditions.Requires(RuntimeInformation.IsOSPlatform(OSPlatform.Windows));

			/**
			 * 4.0.30319.42000
			 * The version we've been testing and targeting.
			 * Other versions will probably work but we're just making sure
			 * todo - determine compatibility
			 */
			Conditions.Requires(Environment.Version == Clr.ClrVersion);

			Conditions.Requires(!GCSettings.IsServerGC);
			
			Conditions.Requires(Type.GetType("Mono.Runtime") == null);

			if (Debugger.IsAttached) {
				Log.Warning("Debugging is enabled: some features may not work correctly");
			}
		}

		public static void Setup()
		{
			CheckCompatibility();
			Console.OutputEncoding = Encoding.Unicode;
			Conditions.Requires(IsSetup);
		}

		public static void Close()
		{
			if (!IsSetup) {
				return;
			}
			
			if (Mem.IsMemoryInUse) {
				Log.Warning("Memory leak: {Count} dangling pointer(s)", Mem.AllocCount);
			}

			Log.Dispose();
			IsSetup = false;
		}
	}
}