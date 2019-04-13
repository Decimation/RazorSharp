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
		public static void Initialize()
		{
			// Init code
			Global.Setup();
			Clr.Setup();

			var appDomain = AppDomain.CurrentDomain;
			appDomain.ProcessExit += (sender, eventArgs) =>
			{
				// SHUT IT DOWN
				SymbolReader.Close();
				Clr.Close();
				Global.Close();
			};
		}
	}
}