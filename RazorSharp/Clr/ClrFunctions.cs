#region

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using RazorSharp.Clr.Structures;
using RazorSharp.Memory;
using RazorSharp.Pointers;
using RazorSharp.Utilities;
using RazorSharp.Utilities.Exceptions;

// ReSharper disable IdentifierTypo

#endregion

// ReSharper disable InconsistentNaming

namespace RazorSharp.Clr
{
	#region

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
			"https://raw.githubusercontent.com/Decimation/RazorSharp/master/RazorSharp/Clr/ClrFunctions.json";

		static ClrFunctions()
		{
			s_setStableEntryPointInterlocked =
				SigScanner.QuickScanDelegateClr<SetStableEntryPointInterlockedDelegate>(
					Environment.Is64BitProcess
						? s_rgStableEntryPointInterlockedSignature
						: s_rgStableEntryPointInterlockedSignature32);

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

		[ClrSigcall]
		internal static Pointer<byte> JIT_GetStaticFieldAddr_Context(FieldDesc* value)
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

		private static readonly byte[] s_rgStableEntryPointInterlockedSignature32 =
		{
			0x55, 0x8B, 0xEC, 0x53, 0x56, 0x57, 0x8B, 0xD9, 0xE8, 0x11, 0x68, 0xF8, 0xFF,
			0x8B,0xCB,0x8B,0xF8, 0xE8, 0x57, 0x41, 0xF8, 0xFF, 0x8B, 0x75, 0x8, 0x8B 
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

		[ClrSigcall]
		internal static uint GetSignatureCorElementType(Pointer<MethodTable> pMT)
		{
			throw new SigcallException();
		}

		internal static Pointer<FieldDesc> FindField(Type t, string name)
		{
			return FindField(t.GetMethodTable(), name);
		}
		
		internal static Pointer<FieldDesc> FindField(Pointer<MethodTable> pMT, string name)
		{
			var module = pMT.Reference.Module;
			var pStr = Mem.AllocString(name);
			var cSig = GetSignatureCorElementType(pMT);
			var field = FindField(pMT, pStr, IntPtr.Zero, cSig, module, 0);
			Mem.FreeString(pStr);
			return field;

		}

		/*
		static FieldDesc * FindField(
			MethodTable *   pMT,
			LPCUTF8         pszName,
			PCCOR_SIGNATURE pSignature,
			DWORD           cSignature,
			Module*         pModule,
			BOOL            bCaseSensitive = TRUE);
		*/
		[ClrSigcall]
		internal static Pointer<FieldDesc> FindField(Pointer<MethodTable> pMT,
		                                             Pointer<byte> pszName,
		                                             Pointer<byte> pSig,
		                                             uint                 cSig,
		                                             Pointer<byte> pModule,
		                                             int                  bCaseSens)
		{
			// pSignature can be NULL to find any field with the given name
			throw new SigcallException();
		}

		#endregion

		internal static T ClrCall<T>(string hex) where T : Delegate
		{
			var fnPtr = SigScanner.QuickScan(CLR_DLL, hex);
			return Marshal.GetDelegateForFunctionPointer<T>(fnPtr);
		}
	}
}