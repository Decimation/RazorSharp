#region

using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using RazorSharp.CoreClr.Structures;
using RazorSharp.Memory;
using RazorSharp.Memory.Calling.Signatures;
using RazorSharp.Memory.Calling.Symbols;
using RazorSharp.Memory.Calling.Symbols.Attributes;
using RazorSharp.Native;
using RazorSharp.Pointers;
using RazorSharp.Utilities;
using RazorSharp.Utilities.Exceptions;

// ReSharper disable IdentifierTypo

#endregion

// ReSharper disable InconsistentNaming

namespace RazorSharp.CoreClr
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
		static ClrFunctions()
		{
			
		}

		/// <summary>
		///     Used just to invoke the type initializer
		/// </summary>
		internal static void Init()
		{
			
			const string FN = "MethodDesc::SetStableEntryPointInterlocked";
			//SetStableEntryPointInterlocked = GetClrFunction<SetStableEntryPointInterlockedDelegate>(FN);


			SetEntryPoint =
				SigScanner.QuickScanDelegate<SetEntryPointDelegate>(
					"clr.dll", "48 89 5C 24 10 48 89 74 24 18 57 48 83");
			

			Symcall.BindQuick(typeof(ClrFunctions));

			Global.Log.Information("ClrFunctions init complete");
			// Conditions.Requires(SignatureCall.IsBound(typeof(ClrFunctions)));
		}

		/// <summary>
		///     Returns the corresponding <see cref="Type" /> for a <see cref="MethodTable" /> pointer.
		/// </summary>
		/// <param name="__struct"><see cref="MethodTable" /> pointer</param>
		/// <returns></returns>
		/// <exception cref="SigcallException">Method has not been bound</exception>
		[ClrSymcall(UseMethodNameOnly = true)]
		internal static Type JIT_GetRuntimeType(void* __struct)
		{
			throw new SigcallException();
		}

		[ClrSymcall(UseMethodNameOnly = true)]
		internal static Pointer<byte> JIT_GetStaticFieldAddr_Context(FieldDesc* value)
		{
			throw new SigcallException();
		}

		#region SetStableEntryPoint

		/// <summary>
		///     We implement <see cref="SetEntryPointDelegate" /> as a <see cref="Delegate" /> initially because
		///     <see cref="MethodDesc.SetStableEntryPointInterlocked" /> has not been bound yet, and in order to bind it
		///     we have to use this function.
		/// </summary>
		/// <param name="__this"><c>this</c> pointer of a <see cref="MethodDesc" /></param>
		/// <param name="pCode">Entry point</param>
		internal delegate int SetEntryPointDelegate(MethodDesc* __this, ulong pCode);

		private static /*readonly*/ SetEntryPointDelegate SetEntryPoint;

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
			var result = SetEntryPoint(pMd, (ulong) pCode);
			Conditions.Assert(result >0);
		}

		#endregion

		[ClrSymcall(UseMethodNameOnly = true)]
		internal static uint GetSignatureCorElementType(Pointer<MethodTable> pMT)
		{
			throw new SigcallException();
		}


		internal static TDelegate GetClrFunctionSig<TDelegate>(string hex) where TDelegate : Delegate
		{
			return SigScanner.QuickScanDelegate<TDelegate>(Clr.CLR_DLL_SHORT, hex);
		}


		internal static TDelegate GetClrFunction<TDelegate>(string name) where TDelegate : Delegate
		{
			return Marshal.GetDelegateForFunctionPointer<TDelegate>(GetClrFunctionAddress(name).Address);
		}

		internal static Pointer<byte> GetClrFunctionAddress(string name)
		{
			return Symbols.GetSymAddress(Clr.ClrPdb.FullName, Clr.CLR_DLL_SHORT, name);
		}

		#region FieldField

		internal static Pointer<FieldDesc> FindField(Type t, string name)
		{
			return FindField(t.GetMethodTable(), name);
		}

		internal static Pointer<FieldDesc> FindField(Pointer<MethodTable> pMT, string name)
		{
			var module = pMT.Reference.Module;
			var pStr   = Marshal.StringToHGlobalAnsi(name);
			var cSig   = GetSignatureCorElementType(pMT);
			var field  = FindField(pMT, pStr, IntPtr.Zero, cSig, module, 0);
			Marshal.FreeHGlobal(pStr);
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
		[ClrSymcall(UseMethodNameOnly = true)]
		internal static Pointer<FieldDesc> FindField(Pointer<MethodTable> pMT,
		                                             Pointer<byte>        pszName,
		                                             Pointer<byte>        pSig,
		                                             uint                 cSig,
		                                             Pointer<byte>        pModule,
		                                             int                  bCaseSens)
		{
			// pSignature can be NULL to find any field with the given name
			throw new SigcallException();
		}

		#endregion
	}
}