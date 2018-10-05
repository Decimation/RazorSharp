#region

using System;
using System.Runtime.InteropServices;
using System.Text;
using RazorSharp.Native.Structures;

#endregion

namespace RazorSharp.Native
{

	/// <summary>
	///     Methods of finding and executing DLL functions:
	///     <para>1. Sig scanning (<see cref="Memory.SigScanner" />)</para>
	///     <list type="bullet">
	///         <item>
	///             <description>Runtime</description>
	///         </item>
	///         <item>
	///             <description>If the function is not DLL-exported</description>
	///         </item>
	///         <item>
	///             <description>Requirements: byte signature of the function</description>
	///         </item>
	///     </list>
	///     <para>2. <see cref="DllImportAttribute" /> attribute</para>
	///     <list type="bullet">
	///         <item>
	///             <description>Compile-time</description>
	///         </item>
	///         <item>
	///             <description>Requirements: function is DLL-exported</description>
	///         </item>
	///     </list>
	///     <para>3. <see cref="GetFunction{TDelegate}" /></para>
	///     <list type="bullet">
	///         <item>
	///             <description>Runtime</description>
	///         </item>
	///         <item>
	///             <description>Requirements: function is DLL-exported</description>
	///         </item>
	///     </list>
	///     <para>4. <see cref="Kernel32.GetProcAddress" /></para>
	///     <list type="bullet">
	///         <item>
	///             <description>Runtime</description>
	///         </item>
	///         <item>
	///             <description>Requirements: function is DLL-exported</description>
	///         </item>
	///     </list>
	/// </summary>
	public static class Functions
	{
		public static TDelegate GetFunction<TDelegate>(string dllName, string fn) where TDelegate : Delegate
		{
			IntPtr hModule = Kernel32.GetModuleHandle(dllName);
			IntPtr hFn     = Kernel32.GetProcAddress(hModule, fn);
			return Marshal.GetDelegateForFunctionPointer<TDelegate>(hFn);
		}

		public static uint GetProcessorType()
		{
			Kernel32.GetNativeSystemInfo(out DbgHelp.SYSTEM_INFO systemInfo);
			uint processorType = Convert.ToUInt32(systemInfo.ProcessorType.ToString(), 16);
			return processorType;
		}

		public static bool IsWow64(IntPtr hProcess)
		{
			bool wow64Process = false;
			Kernel32.IsWow64Process(hProcess, ref wow64Process);
			return wow64Process;
		}

		public static bool LoadModules(IntPtr hProcess, ListModules moduleType)
		{
			//Initialize parameters for EPM
			uint cbNeeded = 0;
			Psapi.EnumProcessModulesEx(hProcess, IntPtr.Zero, 0, out cbNeeded, moduleType);
			long     arraySize  = cbNeeded / IntPtr.Size;
			IntPtr[] hModules   = new IntPtr[arraySize];
			GCHandle gCh        = GCHandle.Alloc(hModules, GCHandleType.Pinned); // Don't forget to free this later
			IntPtr   lphModules = gCh.AddrOfPinnedObject();
			uint     cb         = cbNeeded;
			Psapi.EnumProcessModulesEx(hProcess, lphModules, cb, out cbNeeded, moduleType);
			for (int i = 0; i < arraySize; i++) {
				MODULE_INFO   modInfo          = new MODULE_INFO();
				StringBuilder lpFileName       = new StringBuilder(256);
				StringBuilder lpModuleBaseName = new StringBuilder(32);
				Psapi.GetModuleFileNameExW(hProcess, hModules[i], lpFileName, (uint) lpFileName.Capacity);
				Psapi.GetModuleInformation(hProcess, hModules[i], out modInfo, (uint) Marshal.SizeOf(modInfo));
				Psapi.GetModuleBaseNameW(hProcess, hModules[i], lpModuleBaseName, (uint) lpModuleBaseName.Capacity);
				DbgHelp.SymLoadModuleEx(hProcess, IntPtr.Zero, lpFileName.ToString(), lpModuleBaseName.ToString(),
					modInfo.lpBaseOfDll, (int) modInfo.SizeOfImage, IntPtr.Zero, 0);

			}

			gCh.Free();

			return false;
		}

		public static long UlongToLong(ulong n1)
		{
			byte[] bytes = BitConverter.GetBytes(n1);
			return BitConverter.ToInt64(bytes, 0);
		}

		public static IMAGEHLP_SYMBOL64 GetSymbolFromAddress(IntPtr hProcess, ulong address)
		{
			//Initialize params for SymGetSymFromAddr64
			IMAGEHLP_SYMBOL64 symbol = new IMAGEHLP_SYMBOL64();
			symbol.SizeOfStruct  = (uint) Marshal.SizeOf(symbol);
			symbol.MaxNameLength = 33;

			IntPtr lpSymbol = Marshal.AllocHGlobal(Marshal.SizeOf(symbol));
			Marshal.StructureToPtr(symbol, lpSymbol, false);
			ulong offset = 0;

			DbgHelp.SymGetSymFromAddr64(hProcess, address, offset, lpSymbol);

			symbol = (IMAGEHLP_SYMBOL64) Marshal.PtrToStructure(lpSymbol, typeof(IMAGEHLP_SYMBOL64));
			Marshal.FreeHGlobal(lpSymbol);

			return symbol;
		}

		public static STACKFRAME64 InitializeStackFrame64(AddressMode addrMode, ulong offsetPc, ulong offsetFrame,
			ulong offsetStack, ulong offsetBStore)
		{
			STACKFRAME64 stackFrame = new STACKFRAME64
			{
				AddrPC     = {Mode   = addrMode, Offset = offsetPc},
				AddrReturn = {Mode   = addrMode},
				AddrFrame  = {Mode   = addrMode, Offset   = offsetFrame},
				AddrStack  = {Mode   = addrMode, Offset   = offsetStack},
				AddrBStore = {Offset = offsetBStore, Mode = addrMode}
			};
			return stackFrame;
		}
	}

}