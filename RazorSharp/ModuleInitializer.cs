#region

using System;
using RazorSharp.CoreClr;
using RazorSharp.Native.Symbols;

#endregion

// ReSharper disable UnusedMember.Global

namespace RazorSharp
{
	public static class ModuleInitializer
	{
		/// <summary>
		///     Runs when this module is loaded.
		/// </summary>
		public static void Initialize()
		{
			Global.Log.Information("Loading module");

			// Init code
			Global.Setup();
			Clr.Setup();

			var appDomain = AppDomain.CurrentDomain;
			appDomain.ProcessExit += (sender, eventArgs) =>
			{
				// SHUT IT DOWN
				Global.Log.Information("Unloading module");


				SymbolReader.Close();
				Clr.Close();
				Global.Close();
			};
		}
	}
}