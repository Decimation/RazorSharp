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

		
		/// <summary>
		/// Checks whether there is a <see cref="ProcessModule"/> loaded with the <see cref="ProcessModule.ModuleName"/>
		/// of <paramref name="name"/>.
		/// </summary>
		/// <param name="name"><see cref="ProcessModule.ModuleName"/> to search for</param>
		/// <param name="module"><see cref="ProcessModule"/> to populate if the module is found; <c>null</c> if the module isn't loaded</param>
		/// <returns><c>true</c> if the module is loaded; <c>false</c> otherwise</returns>
		public static bool IsLoaded(string name, out ProcessModule module)
		{
			module = GetModule(name);
			return  module != null;
		}

		/// <summary>
		/// Unloads any <see cref="ProcessModule"/> with the <see cref="ProcessModule.ModuleName"/>
		/// of <paramref name="name"/>.
		/// </summary>
		/// <param name="name"><see cref="ProcessModule.ModuleName"/> to search for</param>
		public static void Unload(string name)
		{
			if (IsLoaded(name, out var mod)) {
				ProcessApi.FreeLibrary(mod.BaseAddress);
			}
		}

		/// <summary>
		/// Loads a module into the current process.
		/// </summary>
		/// <param name="fileName">File name of the module to load</param>
		/// <returns><see cref="ProcessModule"/> of the module which was loaded</returns>
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

		/// <summary>
		/// Returns any <see cref="ProcessModule"/> with the <see cref="ProcessModule.ModuleName"/>
		/// of <paramref name="name"/>.
		/// </summary>
		/// <param name="name"><see cref="ProcessModule.ModuleName"/> to search for</param>
		public static ProcessModule GetModule(string name)
		{
			// todo: I shouldn't have to do this
			if (ModuleInitializer.IsSetup && name == Clr.CLR_DLL_SHORT) {
				return Clr.Value.ClrModule;
			}

			foreach (ProcessModule m in CurrentModules) {
				if (m.ModuleName == name)
					return m;
			}


			return null;
		}
		
		/// <summary>
		/// Returns any <see cref="ProcessModule.BaseAddress"/> with the <see cref="ProcessModule.ModuleName"/>
		/// of <paramref name="name"/>.
		/// </summary>
		/// <param name="name"><see cref="ProcessModule.ModuleName"/> to search for</param>
		public static Pointer<byte> GetBaseAddress(string name)
		{
			// todo: I shouldn't have to do this
			if (ModuleInitializer.IsSetup && name == Clr.CLR_DLL_SHORT) {
				return Clr.Value.ClrModule.BaseAddress;
			}

			var mod = GetModule(name);

			Pointer<byte> baseAddr = mod?.BaseAddress ?? IntPtr.Zero;

			return baseAddr;
		}
		
		/// <summary>
		/// Searches for a <see cref="ProcessModule"/> in which <paramref name="ptr"/> is within its address space.
		/// </summary>
		/// <param name="ptr">Address to search with</param>
		/// <returns><see cref="ProcessModule"/> where <paramref name="ptr"/> is within its address space; <c>null</c>
		/// if a <see cref="ProcessModule"/> could not be found</returns>
		public static ProcessModule FromAddress(Pointer<byte> ptr)
		{
			foreach (ProcessModule module in CurrentModules) {
				var lo = module.BaseAddress;
				var hi = lo + module.ModuleMemorySize;

				if (MemInfo.IsAddressInRange(hi, ptr, lo)) {
					return module;
				}
			}

			return null;
		}
	}
}