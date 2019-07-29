using System.Diagnostics;

namespace RazorSharp.Utilities
{
	internal static class Modules
	{
		/// <summary>
		///     The <see cref="ProcessModuleCollection" /> of the current <see cref="Process" />
		/// </summary>
		internal static ProcessModuleCollection CurrentModules => Process.GetCurrentProcess().Modules;

		/// <summary>
		/// Returns any <see cref="ProcessModule"/> with the <see cref="ProcessModule.ModuleName"/>
		/// of <paramref name="name"/>.
		/// </summary>
		/// <param name="name"><see cref="ProcessModule.ModuleName"/> to search for</param>
		internal static ProcessModule GetModule(string name)
		{
			foreach (ProcessModule m in CurrentModules) {
				if (m.ModuleName == name)
					return m;
			}

			return null;
		}
	}
}