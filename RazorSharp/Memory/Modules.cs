#region

using System.Diagnostics;
using RazorSharp.Pointers;

#endregion

namespace RazorSharp.Memory
{

	// todo: WIP
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

		internal static ProcessModule ScanSector(Pointer<byte> ptr)
		{
			foreach (ProcessModule v in Process.GetCurrentProcess().Modules) {
				if (Mem.IsAddressInRange(v.BaseAddress + v.ModuleMemorySize, ptr.Address, v.BaseAddress)) {
					return v;
				}
			}

			return null;
		}
	}

}