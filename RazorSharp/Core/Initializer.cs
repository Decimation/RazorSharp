using System;
using RazorSharp.CoreClr;
using RazorSharp.CoreClr.Metadata;
using RazorSharp.Import;
using RazorSharp.Interop;
using RazorSharp.Interop.Utilities;
using RazorSharp.Memory;
using RazorSharp.Model;
using SimpleSharp.Diagnostics;

namespace RazorSharp.Core
{
	internal static class Initializer
	{
		private const string CONTEXT = "Init";

		private static bool IsSetup { get; set; }

		/// <summary>
		/// Core CLR types
		/// </summary>
		private static readonly Type[] CoreClrTypes =
		{
			typeof(FunctionFactory),
			typeof(MethodDesc),
			typeof(FieldDesc),
			typeof(MethodTable),
			typeof(GCHeap),
			typeof(TypeHandle)
		};

		/// <summary>
		/// Core objects
		/// </summary>
		private static readonly Closable[] CoreObjects =
		{
			Clr.Value,           // Calls Setup in ctor
			SymbolManager.Value, // Calls Setup in ctor
			ImportManager.Value, // Calls Setup in ctor
			Global.Value,        // Calls Setup in ctor
			Mem.Allocator
		};

		internal static void Setup()
		{
			// Original order: Clr, SymbolManager, Global

			/*foreach (var core in CoreObjects) {
				if (core is Releasable releasable && !releasable.IsSetup) {
					releasable.Setup();
				}
			}

			ImportManager.Value.LoadAll(CoreClrTypes, Clr.Value.Imports);*/

			// Register for domain unload
			
			var appDomain = AppDomain.CurrentDomain;
			appDomain.ProcessExit += (sender, eventArgs) => { Close(); };

			IsSetup = true;
		}


		internal static void Close()
		{
			Conditions.Require(IsSetup);

			// SHUT IT DOWN

			// Original order: Clr, Global, SymbolManager, Mem.Allocator
			
			foreach (var core in CoreObjects) {
				core?.Close();
			}

			IsSetup = false;
		}
	}
}