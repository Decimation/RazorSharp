#region

using System;
using RazorSharp.CoreClr;
using RazorSharp.Native.Symbols;

#endregion

// ReSharper disable UnusedMember.Global

namespace RazorSharp
{
	internal static class ModuleInitializer
	{
		internal static bool IsSetup { get; private set; }

		internal static void GlobalSetup()
		{
			Global.Log.Information("Loading module");

			// Init code
			Global.Setup();
			Clr.Setup();

			IsSetup = true;
		}

		internal static void GlobalClose()
		{
			// SHUT IT DOWN
			Global.Log.Information("Unloading module");

			
			Clr.Close();
			Global.Close();

			IsSetup = false;
		}

		/// <summary>
		///     Runs when this module is loaded.
		/// </summary>
		public static void Initialize()
		{
			GlobalSetup();

			var appDomain = AppDomain.CurrentDomain;
			appDomain.ProcessExit += (sender, eventArgs) =>
			{
				GlobalClose();
			};
		}
	}
}