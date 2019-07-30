using System;
using System.Collections.Generic;
using System.Reflection;
using RazorSharp.CoreClr;
using RazorSharp.CoreClr.Metadata;
using RazorSharp.Import.Attributes;
using RazorSharp.Import.Enums;
using RazorSharp.Memory;
using RazorSharp.Memory.Pointers;
using RazorSharp.Utilities.Security;

namespace RazorSharp.Interop
{
	/// <summary>
	/// Provides functions for resetting and setting the entry point for managed methods.
	/// </summary>
	[ImportNamespace]
	internal static unsafe class FunctionTools
	{
		static FunctionTools()
		{
			ImportMap = new Dictionary<string, Pointer<byte>>();
		}

		[ImportMap]
		private static readonly Dictionary<string, Pointer<byte>> ImportMap;

		/// <summary>
		/// Resets the method represented by <paramref name="mi"/> to its original, blank state.
		/// </summary>
		/// <param name="mi">Method</param>
		[ImportForwardCall(typeof(MethodDesc), nameof(MethodDesc.Reset), ImportCallOptions.Map)]
		internal static void Restore(MethodInfo mi)
		{
			NativeFunctions.CallVoid((void*) ImportMap[nameof(Restore)], Runtime.ResolveHandle(mi).ToPointer());
		}

		/// <summary>
		/// Sets the entry point for the method represented by <paramref name="mi"/> to <paramref name="ptr"/>
		/// </summary>
		/// <param name="mi">Method</param>
		/// <param name="ptr">Function pointer</param>
		/// <returns><c>true</c> if the operation succeeded; <c>false</c> otherwise</returns>
		/// <exception cref="InvalidOperationException">The process is not 64-bit</exception>
		[ImportForwardCall(typeof(MethodDesc), nameof(MethodDesc.SetNativeCodeInterlocked), ImportCallOptions.Map)]
		internal static bool SetEntryPoint(MethodInfo mi, Pointer<byte> ptr)
		{
			if (!Mem.Is64Bit) {
				throw Guard.Require64BitFail(nameof(SetEntryPoint));
			}

			Restore(mi);

			return NativeFunctions.Call<bool>((void*) ImportMap[nameof(SetEntryPoint)],
			                                  mi.MethodHandle.Value.ToPointer(), ptr.ToPointer());
		}
	}
}