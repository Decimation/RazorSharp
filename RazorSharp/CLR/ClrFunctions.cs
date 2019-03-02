#region

using System;
using System.Diagnostics;
using System.Reflection;
using RazorSharp.CLR.Structures;
using RazorSharp.Memory;
using RazorSharp.Utilities.Exceptions;

// ReSharper disable IdentifierTypo

#endregion

// ReSharper disable InconsistentNaming

namespace RazorSharp.CLR
{
	#region

	using CSUnsafe = System.Runtime.CompilerServices.Unsafe;

	#endregion


	/// <summary>
	///     Provides an interface for interacting with and calling native CLR functions
	///     <remarks>
	///         All GC-related functions are WKS, not SVR
	///     </remarks>
	/// </summary>
	internal static unsafe class ClrFunctions
	{
		/// <summary>
		///     <c>clr.dll</c>
		/// </summary>
		internal const string CLR_DLL = "clr.dll";

		private const string JSON_CACHING_URL =
			"https://raw.githubusercontent.com/Decimation/RazorSharp/master/RazorSharp/CLR/ClrFunctions.json";

		static ClrFunctions()
		{
			s_setStableEntryPointInterlocked =
				SigScanner.QuickScanDelegate<SetStableEntryPointInterlockedDelegate>(CLR_DLL,
					s_rgStableEntryPointInterlockedSignature);


			SignatureCall.ReadCacheJsonUrl(new[]
			{
				typeof(FieldDesc), typeof(MethodDesc), typeof(ClrFunctions), typeof(GCHeap)
			}, JSON_CACHING_URL);

			SignatureCall.DynamicBind(typeof(ClrFunctions));
		}

		/// <summary>
		///     Used just to invoke the type initializer
		/// </summary>
		internal static void Init()
		{
			Debug.Assert(SignatureCall.IsBound(typeof(ClrFunctions)));
		}

		/// <summary>
		///     Returns the corresponding <see cref="Type" /> for a <see cref="MethodTable" /> pointer.
		/// </summary>
		/// <param name="__struct"><see cref="MethodTable" /> pointer</param>
		/// <returns></returns>
		/// <exception cref="SigcallException">Method has not been bound</exception>
		[ClrSigcall]
		internal static Type JIT_GetRuntimeType(void* __struct)
		{
			throw new SigcallException();
		}


		#region SetStableEntryPoint

		/// <summary>
		///     We implement <see cref="SetStableEntryPointInterlockedDelegate" /> as a <see cref="Delegate" /> initially because
		///     <see cref="MethodDesc.SetStableEntryPointInterlocked" /> has not been bound yet, and in order to bind it
		///     we have to use this function.
		/// </summary>
		/// <param name="__this"><c>this</c> pointer of a <see cref="MethodDesc" /></param>
		/// <param name="pCode">Entry point</param>
		private delegate long SetStableEntryPointInterlockedDelegate(MethodDesc* __this, ulong pCode);

		private static readonly SetStableEntryPointInterlockedDelegate s_setStableEntryPointInterlocked;

		private static readonly byte[] s_rgStableEntryPointInterlockedSignature =
		{
			0x48, 0x89, 0x5C, 0x24, 0x10, 0x48, 0x89, 0x74, 0x24, 0x18, 0x57, 0x48, 0x83, 0xEC, 0x20, 0x48, 0x8B, 0xFA,
			0x48, 0x8B, 0xF1, 0xE8, 0x1E, 0x57, 0xE7, 0xFF
		};

		/// <summary>
		///     <remarks>
		///         Equal to <see cref="MethodDesc.SetStableEntryPoint" />, but this is implemented via a <see cref="Delegate" />
		///     </remarks>
		/// </summary>
		/// <param name="mi"></param>
		/// <param name="pCode"></param>
		internal static void SetStableEntryPoint(MethodInfo mi, IntPtr pCode)
		{
			var pMd = (MethodDesc*) mi.MethodHandle.Value;
			s_setStableEntryPointInterlocked(pMd, (ulong) pCode);
		}

		#endregion
	}
}