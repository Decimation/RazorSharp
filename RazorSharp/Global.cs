#region

using System;
using System.Text;
using RazorSharp.Clr;
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

		internal static unsafe void Setup()
		{
			// EED2
			//71470000

			// 8B D1 F6 C2 2 F 85 3A 2C 0 0 8B 42 18 8B 48 4 F6 C1 1 F 84 C0 15 9 0 8B 41 FF C3 90 90 51 3B CA F 84 CA 16 18 0 85 C9 74 8 85

			//Console.WriteLine(Meta.GetType<Struct>().Fields[0].Size);


			Conditions.AssertAllEqualQ(Offsets.PTR_SIZE, IntPtr.Size, sizeof(void*), 8);
			Conditions.Assert(Environment.Is64BitProcess);
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