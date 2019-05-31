#region

using System;
using RazorSharp.CoreClr;
using RazorSharp.Import;
using RazorSharp.Native.Symbols;

#endregion

// ReSharper disable UnusedMember.Global

namespace RazorSharp
{
	/// <summary>
	/// Initializers shim. Every type that needs to be set up/closed has:
	/// <list type="bullet">
	/// <item>
	/// <description><see cref="Setup"/> method</description>
	/// </item>
	/// <item>
	/// <description><see cref="Close"/> method</description>
	/// </item>
	/// <item>
	/// <description><see cref="IsSetup"/> property</description>
	/// </item>
	/// </list>
	/// <list type="bullet">
	///         <listheader>Implicit inheritance:</listheader>
	///         <item>
	///             <description>
	///                 <see cref="IReleasable" />
	///             </description>
	///         </item>
	///     </list>
	/// </summary>
	internal static class ModuleInitializer /*: IReleasable */
	{
		internal static bool IsSetup { get; private set; }

		internal static void Setup()
		{
			Global.Log.Information("Loading {Module}", Global.NAME);

			// Init code
			SymbolManager.Setup();
			Global.Setup();
			Clr.Setup();

			IsSetup = true;
		}

		internal static void Close()
		{
			// SHUT IT DOWN
			Global.Log.Information("Unloading {Module}", Global.NAME);


			Clr.Close();
			Symload.UnloadAll(Global.Assembly);
			Symload.Clear();
			Global.Close();
			SymbolManager.Close();
			

			IsSetup = false;
		}

		/// <summary>
		///     Runs when this module is loaded.
		/// </summary>
		public static void Initialize()
		{
			Setup();

			var appDomain = AppDomain.CurrentDomain;
			appDomain.ProcessExit += (sender, eventArgs) =>
			{
				Close();
			};
		}
	}
}