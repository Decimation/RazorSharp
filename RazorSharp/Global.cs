#region

using RazorSharp.Memory;
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