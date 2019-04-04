using System;
using RazorSharp.CoreClr;
using RazorSharp.Native;

namespace RazorSharp
{
	public static class Core
	{
		public static readonly Version Version = new Version(0,1,1,2);
		
		public static void Setup()
		{
			// Init code
			Global.Setup();
			Clr.Setup();
		}

		public static void Close()
		{
			// SHUT IT DOWN
			Symbols.Close();
			Clr.Close();
			Global.Close();
		}
	}
}