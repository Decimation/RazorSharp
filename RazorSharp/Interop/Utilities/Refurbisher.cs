using System;
using System.Collections.Generic;
using System.Reflection;
using RazorSharp.Core;
using RazorSharp.CoreClr;
using RazorSharp.CoreClr.Metadata;
using RazorSharp.Import;
using RazorSharp.Import.Attributes;
using RazorSharp.Import.Enums;
using RazorSharp.Memory;
using RazorSharp.Memory.Pointers;
using RazorSharp.Utilities.Security;

namespace RazorSharp.Interop.Utilities
{
	/// <summary>
	/// Provides functions for resetting and setting the entry point for managed methods.
	/// </summary>
	[ImportNamespace]
	internal static unsafe class Refurbisher
	{
		static Refurbisher()
		{
			ImportManager.Value.Load(typeof(Refurbisher), Clr.Value.Imports);
		}

		[ImportMapDesignation]
		private static readonly ImportMap Imports = new ImportMap();

		/// <summary>
		/// Resets the method represented by <paramref name="mi"/> to its original, blank state.
		/// </summary>
		/// <param name="mi">Method</param>
		[ImportForwardCall(typeof(MethodDesc), nameof(MethodDesc.Reset), ImportCallOptions.Map)]
		internal static void Restore(MethodInfo mi)
		{
			Functions.Native.CallVoid((void*) Imports[nameof(Restore)], Runtime.ResolveHandle(mi).ToPointer());
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

			return Functions.Native.Call<bool>((void*) Imports[nameof(SetEntryPoint)],
			                                  mi.MethodHandle.Value.ToPointer(), ptr.ToPointer());
		}
	}
}