using System;
using NativeSharp.Kernel;
using RazorSharp.Interop.Utilities;

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
				var hModule = Kernel32.GetModuleHandle(dllName);
				var hFn     = Kernel32.GetProcAddress(hModule, fn);
				return DelegateCreator.CreateDelegate<TDelegate>(hFn);
			}
		}
	}
}