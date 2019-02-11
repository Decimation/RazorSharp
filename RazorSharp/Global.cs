using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace RazorSharp
{

	internal static class Global
	{
		internal static readonly Logger Log;

		internal const string CONTEXT_PROP = "Context";
		
		static Global()
		{
			var levelSwitch = new LoggingLevelSwitch();
			
			levelSwitch.MinimumLevel = LogEventLevel.Information;
			
			Log = new LoggerConfiguration()
				.Enrich.FromLogContext()
				.Enrich.WithThreadId()
				.MinimumLevel.ControlledBy(levelSwitch)
				.WriteTo.ColoredConsole(outputTemplate: "[{Timestamp:HH:mm:ss} ({ThreadId}) {Level:u3}] [{Context}] {Message:lj}{NewLine}{Exception}")
				.CreateLogger();
			
			
		}
	}

}