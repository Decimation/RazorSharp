#region

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using RazorSharp.CoreClr;
using RazorSharp.CoreClr.Structures;
using RazorSharp.Import;
using RazorSharp.Import.Attributes;
using RazorSharp.Memory.Pointers;
using RazorSharp.Native.Win32;
using RazorSharp.Utilities;

// ReSharper disable InvalidXmlDocComment

#endregion

namespace RazorSharp.Memory
{
	/// <summary>
	///     Methods of finding and executing DLL functions:
	///     <para>
	///         1. Sig scanning ((deprecated) <see cref="SignatureCall" />) (<see cref="Memory.MemScanner" />)
	///         (deprecated) (<see cref="SigcallAttribute" />)
	///     </para>
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
	///     <para>4. <see cref="ProcessApi.GetProcAddress" /></para>
	///     <list type="bullet">
	///         <item>
	///             <description>Runtime</description>
	///         </item>
	///         <item>
	///             <description>Requirements: function is DLL-exported</description>
	///         </item>
	///     </list>
	///     <para>5. <see cref="Symload" /> (<see cref="SymCallAttribute" />)</para>
	///     <list type="bullet">
	///         <item>
	///             <description>Runtime</description>
	///         </item>
	///         <item>
	///             <description>If the function is not DLL-exported</description>
	///         </item>
	///         <item>
	///             <description>Requirements: symbol/name of the function</description>
	///         </item>
	///     </list>
	/// </summary>
	public static unsafe class Functions
	{
		static Functions()
		{
			const string RESET = "MethodDesc::Reset";
			Reset = Runtime.GetClrFunction<ResetDelegate>(RESET);

			const string SET_NATIVE_CODE = "MethodDesc::SetNativeCodeInterlocked";
			SetNativeCode = Runtime.GetClrFunction<SetNativeCodeInterlockedDelegate>(SET_NATIVE_CODE);
		}
		

		/// <summary>
		///     Gets an exported function
		/// </summary>
		public static TDelegate GetFunction<TDelegate>(string dllName, string fn) where TDelegate : Delegate
		{
			var hModule = ProcessApi.GetModuleHandle(dllName);
			var hFn     = ProcessApi.GetProcAddress(hModule, fn);
			return GetDelegateForFunctionPointer<TDelegate>(hFn);
		}

		#region Delegate functions

		private delegate bool SetNativeCodeInterlockedDelegate(MethodDesc* value, ulong pCode, ulong pExpected = 0);

		private static readonly SetNativeCodeInterlockedDelegate SetNativeCode;

		private delegate void ResetDelegate(MethodDesc* value);

		private static readonly ResetDelegate Reset;

		public static void ResetFunction(MethodInfo mi) => ResetFunction(mi, out _);

		private static void ResetFunction(MethodInfo mi, out MethodDesc* md)
		{
			// We can't use GetMethodDesc here
			md = (MethodDesc*) mi.MethodHandle.Value;
			
			// This will be the first function to fail if clr.pdb and clr.dll are mismatched
			Reset(md);
		}
		
		/// <summary>
		///     <remarks>
		///         Equal to <see cref="MethodDesc.SetEntryPoint" />, but this is implemented via a <see cref="Delegate" />
		///     </remarks>
		/// </summary>
		public static void SetEntryPoint(MethodInfo mi, Pointer<byte> pCode)
		{
			ResetFunction(mi, out var md);

			bool result = SetNativeCode(md, (ulong) pCode);
			
			if (!result) {
				Global.Log.Warning("Possible error setting entry point for {Method} (entry point: {PCode})",
					mi.Name, pCode);
			}
		}

		#endregion

		#region Get delegate

		public static TDelegate GetDelegateForFunctionPointer<TDelegate>(Pointer<byte> ptr) where TDelegate : Delegate
		{
			return (TDelegate) GetDelegateForFunctionPointer(ptr, typeof(TDelegate));
		}

		public static Delegate GetDelegateForFunctionPointer(Pointer<byte> ptr, Type t)
		{
			return Marshal.GetDelegateForFunctionPointer(ptr.Address, t);
		}

		public static void Swap(MethodInfo dest, MethodInfo src)
		{
			var srcCode = src.MethodHandle.GetFunctionPointer();
			SetEntryPoint(dest, srcCode);
		}

		#endregion
	}
}