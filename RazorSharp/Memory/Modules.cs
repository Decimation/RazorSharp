#region

using System;
using System.Diagnostics;
using RazorSharp.CoreClr;
using RazorSharp.Memory.Pointers;
using RazorSharp.Native.Win32;

#endregion

namespace RazorSharp.Memory
{
	/// <summary>
	///     Provides utilities for working with <see cref="ProcessModule" />s
	/// </summary>
	public static class Modules
	{
		/// <summary>
		///     The <see cref="ProcessModuleCollection" /> of the current <see cref="Process" />
		/// </summary>
		internal static ProcessModuleCollection CurrentModules => Process.GetCurrentProcess().Modules;

		public static ProcessModule GetModule(string name)
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

		public static IntPtr GetModuleHandle(string name)
		{
			return Kernel32.GetModuleHandle(name);
		}

		public static IntPtr GetModuleHandle(ProcessModule module)
		{
			return GetModuleHandle(module.ModuleName);
		}

		public static Pointer<byte> GetBaseAddress(string module)
		{
			var pm = GetModule(module);
			return pm.BaseAddress;
		}

		public static Pointer<byte>[] GetAddresses(string module, long[] offset)
		{
			int lim = offset.Length;
			var rg  = new Pointer<byte>[lim];

			Pointer<byte> ptr = GetBaseAddress(module);

			for (int i = 0; i < lim; i++) {
				rg[i] = ptr + offset[i];
			}

			return rg;
		}

		public static Pointer<byte> GetAddress(string module, long offset)
		{
			Pointer<byte> ptr = GetBaseAddress(module);
			return ptr + offset;
		}

		public static Pointer<byte> GetAddress(ProcessModule module, long offset)
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