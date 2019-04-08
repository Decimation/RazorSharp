using System;
using RazorSharp.CoreClr;
using RazorSharp.Native;
using RazorSharp.Native.Symbols;

namespace RazorSharp
{
	public static class Core
	{
		public static readonly Version Version = new Version(0,1,1,3);
		
		// 
		public static void Setup()
		{
			// Init code
			Global.Setup();
			Clr.Setup();
		}

		public static void Close()
		{
			// SHUT IT DOWN
			SymbolReader.Close();
			Clr.Close();
			Global.Close();
		}
	}
}