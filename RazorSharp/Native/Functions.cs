#region

using System;
using System.Runtime.InteropServices;

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
	}

}