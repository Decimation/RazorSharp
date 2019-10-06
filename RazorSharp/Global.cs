using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text;
using JetBrains.Annotations;
using RazorSharp.Model;
using Serilog;
using Serilog.Context;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using SimpleSharp.Diagnostics;
// ReSharper disable MemberCanBeMadeStatic.Global

namespace RazorSharp
{
	/// <summary>
	/// Contains the logger and other useful resources for RazorSharp.
	/// </summary>
	internal sealed class Global : Releasable
	{
		#region Logger

		private const string CONTEXT_PROP = "Context";

		private const string OUTPUT_TEMPLATE =
			"[{Timestamp:HH:mm:ss} {Level:u3}] [{Context}] {Message:lj}{NewLine}{Exception}";

		private const string OUTPUT_TEMPLATE_ALT =
			"[{Timestamp:HH:mm:ss.fff} ({Context}) {Level:u3}] {Message}{NewLine}";

		private const string OUTPUT_TEMPLATE_ALT_12_HR =
			"[{Timestamp:hh:mm:ss.fff} ({Context}) {Level:u3}] {Message}{NewLine}";

#if DEBUG
		private ILogger Log { get; set; }
#endif

		#endregion

		/// <summary>
		/// Name of this assembly.
		/// </summary>
		internal const string NAME = "RazorSharp";

		protected override string Id => nameof(Global);

		#region Singleton

		/// <summary>
		///     Gets an instance of <see cref="Global" />
		/// </summary>
		internal static Global Value { get; private set; } = new Global();

		private Global()
		{
#if DEBUG
			var levelSwitch = new LoggingLevelSwitch
			{
				MinimumLevel = LogEventLevel.Debug
			};

			Log = new LoggerConfiguration()
			     .Enrich.FromLogContext()
			     .MinimumLevel.ControlledBy(levelSwitch)
			     .WriteTo.Console(outputTemplate: OUTPUT_TEMPLATE_ALT_12_HR, theme: SystemConsoleTheme.Colored)
			     .CreateLogger();
#else
//			SuppressLogger();
#endif
		}

		#endregion

		#region Debug logger

		private const string FORMAT_PARAM = "msg";

		[StringFormatMethod(FORMAT_PARAM)]
		internal static void WriteLine(string msg, params object[] args)
		{
			Debug.WriteLine(msg, args: args);
		}

		#endregion
		

		#region Serilog logger extensions
#if DEBUG
		/**
		 * Note: be careful with the logger, as Serilog is only used in debug, and isn't included in
		 * the Release build.
		 */
		
		[Conditional(COND_DEBUG)]
		internal void SuppressLoggerAndClear()
		{
			SuppressLogger();
			Console.Clear();
		}

		[Conditional(COND_DEBUG)]
		internal void SuppressLogger()
		{
			Log = Logger.None;
		}
#endif
		
		[Conditional(COND_DEBUG)]
		private static void ContextLog(string ctx, Action<string, object[]> log, string msg, object[] args)
		{
			if (ctx == null) {
				ctx = String.Empty;
			}
			
			using (LogContext.PushProperty(CONTEXT_PROP, ctx)) {
				log(msg, args);
			}
		}

		private const string COND_DEBUG = "DEBUG";

		/// <summary>
		/// Write a log event with the Debug level, associated exception, and context property.
		/// <see cref="ILogger.Debug(string,object[])"/>
		/// </summary>
		/// <param name="ctx">Context property</param>
		/// <param name="msg">Message template</param>
		/// <param name="args">Property values</param>
		[Conditional(COND_DEBUG)]
		internal void WriteDebug(string ctx, string msg, params object[] args)
		{
			ContextLog(ctx, Log.Debug, msg, args);
		}

		/// <summary>
		/// Write a log event with the Information level, associated exception, and context property.
		/// <see cref="ILogger.Information(string,object[])"/>
		/// </summary>
		/// <param name="ctx">Context property</param>
		/// <param name="msg">Message template</param>
		/// <param name="args">Property values</param>
		[Conditional(COND_DEBUG)]
		internal void WriteInformation(string ctx, string msg, params object[] args)
		{
			ContextLog(ctx, Log.Information, msg, args);
		}

		/// <summary>
		/// Write a log event with the Verbose level, associated exception, and context property.
		/// <see cref="ILogger.Verbose(string,object[])"/>
		/// </summary>
		/// <param name="ctx">Context property</param>
		/// <param name="msg">Message template</param>
		/// <param name="args">Property values</param>
		[Conditional(COND_DEBUG)]
		internal void WriteVerbose(string ctx, string msg, params object[] args)
		{
			ContextLog(ctx, Log.Verbose, msg, args);
		}

		/// <summary>
		/// Write a log event with the Warning level, associated exception, and context property.
		/// <see cref="ILogger.Warning(string,object[])"/>
		/// </summary>
		/// <param name="ctx">Context property</param>
		/// <param name="msg">Message template</param>
		/// <param name="args">Property values</param>
		[Conditional(COND_DEBUG)]
		internal void WriteWarning(string ctx, string msg, params object[] args)
		{
			ContextLog(ctx, Log.Warning, msg, args);
		}

		/// <summary>
		/// Write a log event with the Error level, associated exception, and context property.
		/// <see cref="ILogger.Error(string,object[])"/>
		/// </summary>
		/// <param name="ctx">Context property</param>
		/// <param name="msg">Message template</param>
		/// <param name="args">Property values</param>
		[Conditional(COND_DEBUG)]
		internal void WriteError(string ctx, string msg, params object[] args)
		{
			ContextLog(ctx, Log.Error, msg, args);
		}

		/// <summary>
		/// Write a log event with the Fatal level, associated exception, and context property.
		/// <see cref="ILogger.Fatal(string,object[])"/>
		/// </summary>
		/// <param name="ctx">Context property</param>
		/// <param name="msg">Message template</param>
		/// <param name="args">Property values</param>
		[Conditional(COND_DEBUG)]
		internal void WriteFatal(string ctx, string msg, params object[] args)
		{
			ContextLog(ctx, Log.Fatal, msg, args);
		}

		#endregion

		/// <summary>
		///     Checks compatibility
		/// </summary>
		private void CheckCompatibility()
		{
			/**
			 * RazorSharp is tested on and targets:
			 *
			 * - Windows
			 * - .NET CLR 4.7.2
			 * - Workstation Concurrent GC
			 */
			Conditions.Require(RuntimeInformation.IsOSPlatform(OSPlatform.Windows));

			/**
			 * 4.0.30319.42000
			 * The version we've been testing and targeting.
			 * Other versions will probably work but we're just making sure
			 * todo - determine compatibility
			 */
			Conditions.Require(Environment.Version == Clr.Value.Version);

			Conditions.Require(!GCSettings.IsServerGC);

			Conditions.Require(Type.GetType("Mono.Runtime") == null);

			if (Debugger.IsAttached) {
				WriteWarning(NAME,"Debugging is enabled!");
			}
		}
		


		public override void Setup()
		{
			CheckCompatibility();
			Console.OutputEncoding = Encoding.Unicode;

			base.Setup();
		}

		/// <summary>
		///     Disposes the logger
		/// </summary>
		public override void Close()
		{
			Conditions.Require(IsSetup);
#if DEBUG
			if (Log is Logger logger) {
				logger.Dispose();
			}

			Log = null;
#endif
			// Delete instance
			Value = null;

			base.Close();
		}
	}
}