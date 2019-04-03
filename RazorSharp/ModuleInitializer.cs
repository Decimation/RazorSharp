using System;
using RazorSharp.CoreClr;
using RazorSharp.Native;

// ReSharper disable UnusedMember.Global

namespace RazorSharp
{
	public static class ModuleInitializer
	{
		public static void Initialize()
		{
			// Init code
			Global.Setup();
			Clr.Setup();
			
			var appDomain = AppDomain.CurrentDomain;
			appDomain.ProcessExit += (sender, eventArgs) =>
			{
				// SHUT IT DOWN
				Symbols.Close();
				Clr.Close();
				Global.Close();
			};
		}
	}
}