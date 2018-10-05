using System;
using System.Runtime.InteropServices;
using System.Text;

namespace RazorSharp.Native
{
	[StructLayout(LayoutKind.Sequential)]
	public struct MODULE_INFO
	{
		public IntPtr lpBaseOfDll;
		public uint   SizeOfImage;
		public IntPtr EntryPoint;
	}

	[Flags]
	public enum ListModules : uint
	{
		Default = 0x0,
		_32Bit  = 0x01,
		_64Bit  = 0x02,
		All     = 0x03,
	}

	public static class Psapi
	{
		private const string PsapiLib = "psapi.dll";

		//EnumProcessModulesEx
		[DllImport(PsapiLib, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool EnumProcessModulesEx(IntPtr hProcess, [Out] IntPtr lphModuleArray, uint cb,
			out uint cbNeeded, ListModules FilterFlags);

		//GetModuleInformation
		[DllImport(PsapiLib, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GetModuleInformation(IntPtr hProcess, IntPtr hModule, out MODULE_INFO lpModInfo, uint cb);

		//GetModuleBaseNameW
		[DllImport(PsapiLib, SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern uint GetModuleBaseNameW(IntPtr hProcess, IntPtr hModule, StringBuilder lpBaseName, uint nSize);

		//GetModuleFileNameExW
		[DllImport(PsapiLib, SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern uint GetModuleFileNameExW(IntPtr hProcess, IntPtr hModule, StringBuilder lpFilename, uint nSize);

		////GetMappedFileNameW
		[DllImport(PsapiLib, SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern uint GetMappedFileNameW(IntPtr hProcess, IntPtr lpAddress, StringBuilder lpFilename, uint nSize);
	}

}