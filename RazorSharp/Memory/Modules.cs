#region

using System;
using System.Diagnostics;
using System.Linq;
using SimpleSharp.Diagnostics;
using RazorSharp.CoreClr;
using RazorSharp.Memory.Pointers;
using RazorSharp.Native;
using RazorSharp.Native.Win32;
using SimpleSharp.Strings;

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

		internal static NativeModule[] CurrentNativeModules
			=> ProcessApi.GetProcessModules(Process.GetCurrentProcess());

		// todo: ProcessModules are actually updated when they are natively loaded
		// todo: remove CurrentNativeModules

		public static bool IsLoaded(string name)
		{
			return GetModule(name) != null;
		}

		public static void UnloadIfLoaded(string name)
		{
			var mod = GetModule(name);

			if (mod != null) {
				ProcessApi.FreeLibrary(mod.BaseAddress);
			}
		}

		public static ProcessModule LoadModule(string fileName)
		{
			//var ptr = ProcessApi.LoadLibrary(fileName);
			//return CurrentNativeModules.First(m => m.FileName == fileName);
			var ptr = ProcessApi.LoadLibrary(fileName);

			foreach (ProcessModule m in CurrentModules) {
				if (m.FileName == fileName)
					return m;
			}

			return null;
		}

		public static ProcessModule GetModule(string name)
		{
			// todo: I shouldn't have to do this
			if (ModuleInitializer.IsSetup && Clr.Value.IsSetup && name == Clr.CLR_DLL_SHORT) {
				return Clr.Value.ClrModule;
			}

			foreach (ProcessModule m in CurrentModules) {
				if (m.ModuleName == name)
					return m;
			}


			return null;
		}

		public static NativeModule GetNativeModule(string name)
		{
			foreach (var pair in CurrentNativeModules) {
				if (pair.Name == name) {
					return pair;
				}
			}

			return NativeModule.NullModule;
		}


		public static Pointer<byte> GetBaseAddress(string name)
		{
			if (name == Clr.CLR_DLL_SHORT) {
				return Clr.Value.ClrModule.BaseAddress;
			}

			var mod = GetModule(name);

			Pointer<byte> baseAddr = mod?.BaseAddress ?? IntPtr.Zero;


//			Global.Log.Debug("Base addr for {Name} {Addr}", name, Hex.ToHex(baseAddr));

			return baseAddr;
		}

		private static Pointer<byte>[] GetAddressesInternal(Pointer<byte> baseAddr, long[] offset)
		{
			var rg = new Pointer<byte>[offset.Length];

			for (int i = 0; i < rg.Length; i++) {
				rg[i] = baseAddr + offset[i];
			}

			return rg;
		}

		public static Pointer<byte>[] GetAddresses(string module, long[] offset)
		{
			return GetAddresses(GetModule(module), offset);
		}

		public static Pointer<byte>[] GetAddresses(ProcessModule module, long[] offset)
		{
			return GetAddressesInternal(module.BaseAddress, offset);
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