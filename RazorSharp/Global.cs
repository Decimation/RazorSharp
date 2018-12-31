using Serilog;
using Serilog.Core;

namespace RazorSharp
{

	internal static class Global
	{
		internal static readonly Logger GLogger;

		static Global()
		{
			var levelSwitch = new LoggingLevelSwitch();
			GLogger = new LoggerConfiguration()
				.MinimumLevel.ControlledBy(levelSwitch)
				.WriteTo.ColoredConsole()
				.CreateLogger();
		}
	}

}