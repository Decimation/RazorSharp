using System;
using JetBrains.Annotations;
using RazorSharp.CoreClr.Metadata;
using RazorSharp.CoreClr.Structures;
using RazorSharp.Import;
using RazorSharp.Interop;
using RazorSharp.Memory;
using RazorSharp.Model;

namespace RazorSharp
{
	/// <summary>
	///     Initializers shim. Every type that needs to be set up/closed implements <see cref="Releasable"/>.
	///     <list type="bullet">
	///         <listheader>Implicit inheritance:</listheader>
	///         <item>
	///             <description>
	///                 <see cref="Releasable" />
	///             </description>
	///         </item>
	///     </list>
	/// </summary>
	internal static class ModuleInitializer /*: Releasable */
	{
		private const string CONTEXT = "Init";
		
		internal static bool IsSetup { get; private set; }

		/// <summary>
		/// Core CLR types
		/// </summary>
		private static readonly Type[] CoreClrTypes =
		{
			typeof(Functions),
			typeof(FunctionTools),
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
		private static readonly Releasable[] CoreInitializers =
		{
			Clr.Value,
			SymbolManager.Value,
			ImportManager.Value,
			Global.Value,
		};

		private static void Setup()
		{
			Global.Value.WriteInformation(CONTEXT, "Loading {Module}", Global.NAME);

			// Init code
			
			// Original order: Clr, SymbolManager, Global

			foreach (var core in CoreInitializers) {
				core.Setup();
			}
			
			ImportManager.Value.LoadAll(CoreClrTypes, Clr.Value.ClrSymbols);

			IsSetup = true;
		}

		private static void Close()
		{
			// SHUT IT DOWN
			Global.Value.WriteInformation(CONTEXT, "Unloading {Module}", Global.NAME);
			
			// Original order: Clr, Global, SymbolManager, Mem.Allocator

			foreach (var core in CoreInitializers) {
				core.Close();
			}

			Mem.Allocator.Close();
			
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