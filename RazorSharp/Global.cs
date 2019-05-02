#region

using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using RazorCommon.Diagnostics;
using RazorSharp.CoreClr;
using RazorSharp.Memory;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

#endregion

namespace RazorSharp
{
	/// <summary>
	///     The core of RazorSharp. Contains the logger and <see cref="Setup" />
	/// </summary>
	internal static class Global
	{
		internal const string CONTEXT_PROP = "Context";

		private const string OUTPUT_TEMPLATE =
			"[{Timestamp:HH:mm:ss} {Level:u3}] [{Context}] {Message:lj}{NewLine}{Exception}";

		private const string OUTPUT_TEMPLATE_ALT =
			"[{Timestamp:HH:mm:ss.fff} ({Context}) {Level:u3}] {Message}{NewLine}";

		internal static readonly Logger Log;

		internal static readonly Assembly Assembly;

		internal static bool IsSetup { get; private set; }

		/// <summary>
		///     Sets up the logger and other values
		/// </summary>
		static Global()
		{
			var levelSwitch = new LoggingLevelSwitch
			{
				MinimumLevel = LogEventLevel.Debug
			};

			Log = new LoggerConfiguration()
			     .Enrich.FromLogContext()
			     .MinimumLevel.ControlledBy(levelSwitch)
			     .WriteTo.Console(outputTemplate: OUTPUT_TEMPLATE_ALT, theme: SystemConsoleTheme.Colored)
			     .CreateLogger();

			const string ASM_STR = "RazorSharp";

			Assembly = Assembly.Load(ASM_STR);
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
			Conditions.Require(Mem.Is64Bit);
			Conditions.Require(RuntimeInformation.IsOSPlatform(OSPlatform.Windows));

			/**
			 * 4.0.30319.42000
			 * The version we've been testing and targeting.
			 * Other versions will probably work but we're just making sure
			 * todo - determine compatibility
			 */
			Conditions.Require(Environment.Version == Clr.ClrVersion);

			Conditions.Require(!GCSettings.IsServerGC);

			Conditions.Require(Type.GetType("Mono.Runtime") == null);

			if (Debugger.IsAttached) {
				Log.Warning("Debugging is enabled: some features may not work correctly");
			}
		}

		/// <summary>
		///     Checks compatibility
		/// </summary>
		internal static void Setup()
		{
			CheckCompatibility();
			Console.OutputEncoding = Encoding.Unicode;
			IsSetup                = true;
		}

		/// <summary>
		///     Disposes the logger and checks for any memory leaks
		/// </summary>
		internal static void Close()
		{
			Conditions.Require(IsSetup);

			if (Mem.IsMemoryInUse) {
				Log.Warning("Memory leak: {Count} dangling pointer(s)", Mem.AllocCount);
			}

			Log.Dispose();
			IsSetup = false;
		}
	}
}