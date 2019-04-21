#region

using System;
using System.Diagnostics;
using System.Linq;
using RazorCommon.Diagnostics;
using RazorSharp.CoreClr;
using RazorSharp.Memory.Pointers;
using RazorSharp.Native;
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

		internal static NativeModule[] CurrentNativeModules
			=> ProcessApi.GetProcessModules(Process.GetCurrentProcess());

		public static bool IsLoaded(string name)
		{
			foreach (var module in CurrentNativeModules) {
				if (module.Name == name) {
					return true;
				}
			}
			
			return GetModule(name) != null;
		}

		public static NativeModule LoadModule(string fileName)
		{
			var ptr = ProcessApi.LoadLibrary(fileName);
			return CurrentNativeModules.First(m => m.BaseAddress == ptr);
		}
		
		public static ProcessModule GetModule(string name)
		{
			// todo: I shouldn't have to do this
			if (name == Clr.CLR_DLL_SHORT && Clr.IsSetup) {
				return Clr.ClrModule;
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

		public static IntPtr GetModuleHandle(string name)
		{
			return ProcessApi.GetModuleHandle(name);
		}

		public static IntPtr GetModuleHandle(ProcessModule module)
		{
			return GetModuleHandle(module.ModuleName);
		}

		public static Pointer<byte> GetBaseAddress(string module)
		{
			Pointer<byte> ptr;

			var pm = GetModule(module);

			ptr = pm?.BaseAddress ?? GetNativeModule(module).BaseAddress;

			if (ptr.IsNull) {
				string msg = String.Format("Module \"{0}\" is not loaded or the base address could not be retrieved",
				                           module);
				throw new Exception(msg);
			}

			return ptr;
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