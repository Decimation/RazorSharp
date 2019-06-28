#region

using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text;
using RazorSharp.Components;
using SimpleSharp.Diagnostics;
using RazorSharp.CoreClr;
using RazorSharp.Memory;
using Serilog;
using Serilog.Context;
using Serilog.Core;

#endregion

namespace RazorSharp
{
	/// <summary>
	///     The core of RazorSharp. Contains the logger and such.
	/// <para></para>
	/// <list type="bullet">
	///         <listheader>Implicit inheritance:</listheader>
	///         <item>
	///             <description>
	///                 <see cref="Releasable" />
	///             </description>
	///         </item>
	///     </list>
	/// </summary>
	internal static class Global /*: Releasable */
	{
		#region Logger

		private const string CONTEXT_PROP = "Context";

		private const string OUTPUT_TEMPLATE =
			"[{Timestamp:HH:mm:ss} {Level:u3}] [{Context}] {Message:lj}{NewLine}{Exception}";

		private const string OUTPUT_TEMPLATE_ALT =
			"[{Timestamp:HH:mm:ss.fff} ({Context}) {Level:u3}] {Message}{NewLine}";

		internal static ILogger Log { get; private set; }

		#endregion

		/// <summary>
		/// Name of this module.
		/// </summary>
		internal const string NAME = "RazorSharp";
		
		internal static readonly Assembly Assembly;

		internal static bool IsSetup { get; private set; }

		/// <summary>
		///     Sets up the logger and other values
		/// </summary>
		static Global()
		{
#if DEBUG
			var levelSwitch = new LoggingLevelSwitch
			{
				MinimumLevel = LogEventLevel.Verbose
			};


			Log = new LoggerConfiguration()
			     .Enrich.FromLogContext()
			     .MinimumLevel.ControlledBy(levelSwitch)
			     .WriteTo.Console(outputTemplate: OUTPUT_TEMPLATE_ALT, theme: SystemConsoleTheme.Colored)
			     .CreateLogger();
#else
			SuppressLogger();
#endif

			Assembly = Assembly.Load(NAME);
		}

		internal static void SuppressLogger()
		{
			Log = Logger.None;
		}

		internal static void ContextLog(string prop, Action fn)
		{
			using (LogContext.PushProperty(CONTEXT_PROP, prop)) {
				fn();
			}
		}

		private static void CheckCompatibility()
		{
			/**
			 * RazorSharp is tested on and targets:
			 *
			 * - x64
			 * - Windows
			 * - .NET CLR 4.7.2
			 * - Workstation Concurrent GC
			 *
			 */
			Conditions.Require(MemInfo.Is64Bit);
			Conditions.Require(RuntimeInformation.IsOSPlatform(OSPlatform.Windows));

			/**
			 * 4.0.30319.42000
			 * The version we've been testing and targeting.
			 * Other versions will probably work but we're just making sure
			 * todo - determine compatibility
			 */
			Conditions.Require(Environment.Version == Clr.Value.ClrVersion);

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

			if (Log is Logger logger) {
				logger.Dispose();
			}

			Log = null;

			IsSetup = false;
		}
	}
}