using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using RazorSharp.CoreClr;
using RazorSharp.Memory.Pointers;
using RazorSharp.Native.Win32.Structures;

namespace RazorSharp.Native.Win32
{
	internal static unsafe class ProcessApi
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

		

		public static NativeModuleInfo GetModuleInfo(ProcessModule module)
		{
			return GetModuleInfo(Process.GetCurrentProcess(), module);
		}

		public static NativeModuleInfo GetModuleInfo(Process proc, ProcessModule module)
		{
			var hProc = Kernel32.OpenProcess(proc);
			var hMod  = GetModuleHandle(module);
			return GetModuleInfo(hProc, hMod);
		}

		public static NativeModuleInfo GetModuleInfo(IntPtr hProc, IntPtr hModule)
		{
			NativeModuleInfo moduleInfo = default;

			var pMod = new IntPtr(&moduleInfo);

			NativeHelp.Call(GetModuleInformation(hProc,
			                                     hModule,
			                                     pMod,
			                                     (uint) Marshal.SizeOf<NativeModuleInfo>()));

			Kernel32.CloseHandle(hProc);

			return moduleInfo;
		}

		[DllImport(PSAPI_DLL)]
		private static extern bool GetModuleInformation(IntPtr hProcess, IntPtr hModule, IntPtr lpmodinfo, uint cb);

		internal static NativeModule[] GetProcessModules(Process p)
		{
			var pairs = new List<NativeModule>();


			// Setting up the variable for the second argument for EnumProcessModules
			var hMods = new IntPtr[Constants.KIBIBYTE];

			var gch      = GCHandle.Alloc(hMods, GCHandleType.Pinned); // Don't forget to free this later
			var pModules = gch.AddrOfPinnedObject();

			// Setting up the rest of the parameters for EnumProcessModules
			uint uiSize = (uint) (IntPtr.Size * hMods.Length);

			if (EnumProcessModules(p.Handle, pModules, uiSize, out uint cbNeeded) == 1) {
				// To determine how many modules were enumerated by the call to EnumProcessModules,
				// divide the resulting value in the lpcbNeeded parameter by sizeof(HMODULE).
				int uiTotalNumberOfModules = (int) (cbNeeded / IntPtr.Size);

				for (int i = 0; i < uiTotalNumberOfModules; i++) {
					var strbld = new StringBuilder(Constants.KIBIBYTE);

					GetModuleFileNameEx(p.Handle, hMods[i], strbld, strbld.Capacity);

					pairs.Add(new NativeModule(strbld.ToString(), hMods[i]));
				}
			}

			// Must free the GCHandle object
			gch.Free();

			return pairs.ToArray();
		}


		/// <summary>
		///     Retrieves a module handle for the specified module. The module must have been loaded by the calling process.
		/// </summary>
		[DllImport(Kernel32.KERNEL32_DLL, CharSet = CharSet.Auto)]
		internal static extern IntPtr GetModuleHandle(string lpModuleName);

		internal static IntPtr GetModuleHandle(ProcessModule module) => GetModuleHandle(module.ModuleName);

		/// <summary>
		///     Retrieves the address of an exported function or variable from the specified dynamic-link library (DLL).
		/// </summary>
		[DllImport(Kernel32.KERNEL32_DLL, CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
		internal static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

		#region Library

		/// <returns>If the function succeeds, the return value is a handle to the module.</returns>
		[DllImport(Kernel32.KERNEL32_DLL, SetLastError = true)]
		internal static extern IntPtr LoadLibrary(string lpFileName);

		[DllImport(Kernel32.KERNEL32_DLL, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool FreeLibrary(IntPtr hModule);

		#endregion
	}
}