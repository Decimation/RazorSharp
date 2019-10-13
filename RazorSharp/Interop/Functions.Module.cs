using System;
using RazorSharp.Interop.Utilities;
using RazorSharp.Native.Win32;

namespace RazorSharp.Interop
{
	public static partial class Functions
	{
		/// <summary>
		/// Provides methods for accessing DLL-exported functions.
		/// </summary>
		public static class Module
		{
			/// <summary>
			///     Gets an exported function
			/// </summary>
			public static TDelegate FindExportedFunction<TDelegate>(string dllName, string fn) where TDelegate : Delegate
			{
				var hModule = NativeWin32.Kernel.GetModuleHandle(dllName);
				var hFn     = NativeWin32.Kernel.GetProcAddress(hModule, fn);
				return DelegateCreator.CreateDelegate<TDelegate>(hFn);
			}
		}
	}
}