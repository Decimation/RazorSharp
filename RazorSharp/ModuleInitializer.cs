using System;
using JetBrains.Annotations;
using RazorSharp.CoreClr.Metadata;
using RazorSharp.CoreClr.Structures;
using RazorSharp.Import;
using RazorSharp.Interop;
using RazorSharp.Interop.Utilities;
using RazorSharp.Memory;
using RazorSharp.Model;
using SimpleSharp.Diagnostics;

namespace RazorSharp
{
	/// <summary>
	///     Initializers shim. Every type that needs to be set up/closed implements <see cref="Releasable"/>.
	/// This runs on module load.
	/// </summary>
	internal static class ModuleInitializer /*: Releasable */
	{
		private const string CONTEXT = "Init";

		private static bool IsSetup { get; set; }

		/// <summary>
		/// Core CLR types
		/// </summary>
		private static readonly Type[] CoreClrTypes =
		{
			typeof(Refurbisher),
			typeof(DelegateCreator),
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
			Clr.Value,
			SymbolManager.Value,
			ImportManager.Value,
			Global.Value,
			Mem.Allocator
		};

		private static void Setup()
		{
			Global.Value.WriteInformation(CONTEXT, "Loading {Module}", Global.NAME);

			// Init code

			// Original order: Clr, SymbolManager, Global

			foreach (var core in CoreObjects) {
				if (core is Releasable releasable) {
					releasable.Setup();
				}
			}

			ImportManager.Value.LoadAll(CoreClrTypes, Clr.Value.Imports);

			IsSetup = true;
		}

		private static void Close()
		{
			Conditions.Require(IsSetup);
			
			// SHUT IT DOWN
			Global.Value.WriteInformation(CONTEXT, "Unloading {Module}", Global.NAME);

			// Original order: Clr, Global, SymbolManager, Mem.Allocator

			foreach (var core in CoreObjects) {
				core.Close();
			}

			IsSetup = false;
		}

		/// <summary>
		///     Runs when this module is loaded.
		/// </summary>
		[UsedImplicitly]
		public static void Initialize()
		{
			Setup();

			var appDomain = AppDomain.CurrentDomain;
			appDomain.ProcessExit += (sender, eventArgs) => { Close(); };
		}
	}
}