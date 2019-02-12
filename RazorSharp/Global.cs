using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace RazorSharp
{
	internal static class Global
	{
		internal const string CONTEXT_PROP = "Context";

		private const string OUTPUT_TEMPLATE =
			"[{Timestamp:HH:mm:ss} ({ThreadId}) {Level:u3}] [{Context}] {Message:lj}{NewLine}{Exception}";

		internal static readonly Logger Log;

		static Global()
		{
			var levelSwitch = new LoggingLevelSwitch
			{
				MinimumLevel = LogEventLevel.Information
			};


			Log = new LoggerConfiguration()
				.Enrich.FromLogContext()
				.Enrich.WithThreadId()
				.MinimumLevel.ControlledBy(levelSwitch)
				.WriteTo.ColoredConsole(outputTemplate: OUTPUT_TEMPLATE)
				.CreateLogger();
		}
	}
}