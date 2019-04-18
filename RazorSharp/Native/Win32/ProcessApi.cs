using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using RazorSharp.Memory.Pointers;

namespace RazorSharp.Native.Win32
{
	internal static class ProcessApi
	{
		private const string PSAPI_DLL = "psapi.dll";

		[DllImport(PSAPI_DLL, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
		internal static extern int EnumProcessModules(IntPtr       hProcess,
		                                              [Out] IntPtr lphModule,
		                                              uint         cb,
		                                              out uint     lpcbNeeded);

		[DllImport(PSAPI_DLL)]
		private static extern uint GetModuleFileNameEx(IntPtr                                           hProcess,
		                                               IntPtr                                           hModule,
		                                               [Out]                              StringBuilder lpBaseName,
		                                               [In] [MarshalAs(UnmanagedType.U4)] int           nSize);

		internal static (string, Pointer<byte>)[] GetProcessModules(Process p)
		{
			var pairs = new List<(string, Pointer<byte>)>();


			// Setting up the variable for the second argument for EnumProcessModules
			var hMods = new IntPtr[1024];

			var gch      = GCHandle.Alloc(hMods, GCHandleType.Pinned); // Don't forget to free this later
			var pModules = gch.AddrOfPinnedObject();

			// Setting up the rest of the parameters for EnumProcessModules
			uint uiSize = (uint) (Marshal.SizeOf(typeof(IntPtr)) * (hMods.Length));

			if (EnumProcessModules(p.Handle, pModules, uiSize, out uint cbNeeded) == 1) {
				// To determine how many modules were enumerated by the call to EnumProcessModules,
				// divide the resulting value in the lpcbNeeded parameter by sizeof(HMODULE).
				int uiTotalNumberofModules = (int) (cbNeeded / (Marshal.SizeOf(typeof(IntPtr))));

				for (int i = 0; i < uiTotalNumberofModules; i++) {
					var strbld = new StringBuilder(1024);

					GetModuleFileNameEx(p.Handle, hMods[i], strbld, (strbld.Capacity));

					pairs.Add((strbld.ToString(), hMods[i]));
				}
			}

			// Must free the GCHandle object
			gch.Free();

			return pairs.ToArray();
		}
	}
}