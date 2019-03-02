#region

using System;
using System.Runtime.InteropServices;
using RazorSharp.Memory;
using RazorSharp.Native.Enums;

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
	public static class NativeFunctions
	{
		public static TDelegate GetFunction<TDelegate>(string dllName, string fn) where TDelegate : Delegate
		{
			var hModule = Kernel32.GetModuleHandle(dllName);
			var hFn     = Kernel32.GetProcAddress(hModule, fn);
			return Marshal.GetDelegateForFunctionPointer<TDelegate>(hFn);
		}

		

		public static void CodeFree(IntPtr fn)
		{
			if (!Kernel32.VirtualFree(fn, 0, FreeTypes.Release)) throw new Exception();
		}

		public static IntPtr CodeAlloc(byte[] opCodes)
		{
			Kernel32.GetNativeSystemInfo(out var si);

			// VirtualAlloc(nullptr, page_size, MEM_COMMIT, PAGE_READWRITE);

			// @formatter:off
			var alloc = Kernel32.VirtualAlloc(IntPtr.Zero, (UIntPtr) si.PageSize, AllocationType.Commit,MemoryProtection.ReadWrite);
			// @formatter:on

			Mem.Copy(alloc, opCodes);

			// VirtualProtect(buffer, code.size(), PAGE_EXECUTE_READ, &dummy);

			Kernel32.VirtualProtect(alloc, (uint) opCodes.Length, MemoryProtection.ExecuteRead, out _);

			return alloc;
		}
	}
}