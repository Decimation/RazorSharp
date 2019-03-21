#region

using System.Collections.Generic;
using System.Diagnostics;
using RazorSharp.Pointers;

#endregion

namespace RazorSharp.Memory
{
	// todo: WIP
	internal static class Modules
	{

		private static ProcessModuleCollection ProcessModules {
			get {
				return Process.GetCurrentProcess().Modules;
			}
		}

		
		
		internal static ProcessModule GetModule(string name)
		{
			foreach (ProcessModule m in ProcessModules)
				if (m.ModuleName == name)
					return m;

			return null;
		}

		internal static ProcessModule ScanSector(Pointer<byte> ptr)
		{
			foreach (ProcessModule v in ProcessModules)
				if (Mem.IsAddressInRange(v.BaseAddress + v.ModuleMemorySize, ptr.Address, v.BaseAddress))
					return v;

			return null;
		}

		internal static Pointer<byte> GetBaseAddress(string module)
		{
			var           pm  = GetModule(module);
			Pointer<byte> ptr = pm.BaseAddress;
			return ptr;
		}
		
		internal static IEnumerable<Pointer<byte>> GetAddresses(string module, long[] offset)
		{
			var ptr = GetBaseAddress(module);
			foreach (long ofs in offset) {
				yield return ptr + ofs;
			}
		}
		
		internal static Pointer<byte> GetAddress(string module, long offset)
		{
			var ptr = GetBaseAddress(module);
			return ptr + offset;
		}

		public static ProcessModule FromAddress(Pointer<byte> ptr)
		{
			foreach (ProcessModule module in ProcessModules) {
				var lo = module.BaseAddress;
				var hi = lo + module.ModuleMemorySize;

				if (Mem.IsAddressInRange(hi, ptr, lo)) {
					return module;
				}
			}

			return null;
		}
	}
}