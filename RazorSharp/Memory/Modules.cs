#region

using System.Diagnostics;

#endregion

namespace RazorSharp.Memory
{

	internal static class Modules
	{
		internal static ProcessModule GetModule(string name)
		{
			foreach (ProcessModule m in Process.GetCurrentProcess().Modules) {
				if (m.ModuleName == name) {
					return m;
				}
			}

			return null;
		}
	}

}