#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using RazorCommon.Diagnostics;
using RazorSharp.CoreClr;
using RazorSharp.Native;
using RazorSharp.Pointers;

#endregion

namespace RazorSharp.Memory
{
	/// <summary>
	///     Provides utilities for working with <see cref="ProcessModule" />s
	/// </summary>
	internal static class Modules
	{
		/// <summary>
		///     The <see cref="ProcessModuleCollection" /> of the current <see cref="Process" />
		/// </summary>
		internal static ProcessModuleCollection CurrentModules => Process.GetCurrentProcess().Modules;

		internal static ProcessModule GetModule(string name)
		{
			// todo: I shouldn't have to do this
			if (name == Clr.CLR_DLL_SHORT && Clr.IsSetup) {
				return Clr.ClrModule;
			}

			foreach (ProcessModule m in CurrentModules)
				if (m.ModuleName == name)
					return m;

			return null;
		}

		internal static IntPtr GetModuleHandle(string name) => Kernel32.GetModuleHandle(name);

		internal static IntPtr GetModuleHandle(ProcessModule module) => GetModuleHandle(module.ModuleName);

		internal static Pointer<byte> GetBaseAddress(string module)
		{
			var pm = GetModule(module);
			return pm.BaseAddress;
		}

		internal static IEnumerable<Pointer<byte>> GetAddresses(string module, long[] offset)
		{
			Pointer<byte> ptr = GetBaseAddress(module);
			foreach (long ofs in offset) {
				yield return ptr + ofs;
			}
		}

		internal static Pointer<byte> GetAddress(string module, long offset)
		{
			Pointer<byte> ptr = GetBaseAddress(module);
			return ptr + offset;
		}

		internal static Pointer<byte> GetAddress(ProcessModule module, long offset)
		{
			Pointer<byte> ptr = module.BaseAddress;
			return ptr + offset;
		}

		public static ProcessModule FromAddress(Pointer<byte> ptr)
		{
			foreach (ProcessModule module in CurrentModules) {
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