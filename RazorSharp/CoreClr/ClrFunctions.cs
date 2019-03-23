#region

using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using RazorCommon;
using RazorSharp.CoreClr.Structures;
using RazorSharp.Memory;
using RazorSharp.Memory.Calling.Signatures;
using RazorSharp.Memory.Calling.Signatures.Attributes;
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
			const string FN = "MethodDesc::SetStableEntryPointInterlocked";
			SetEntryPoint = GetClrFunction<SetEntryPointDelegate>(FN);
			Symcall.BindQuick(typeof(ClrFunctions));
			Global.Log.Information("ClrFunctions init complete");
		}

		

		/// <summary>
		///     Returns the corresponding <see cref="Type" /> for a <see cref="MethodTable" /> pointer.
		/// </summary>
		/// <param name="value"><see cref="MethodTable" /> pointer</param>
		/// <returns></returns>
		/// <exception cref="SigcallException">Method has not been bound</exception>
		[ClrSymcall(UseMethodNameOnly = true)]
		internal static Type JIT_GetRuntimeType(void* value)
		{
			throw new SigcallException(nameof(JIT_GetRuntimeType));
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
		internal delegate long SetEntryPointDelegate(MethodDesc* value, ulong pCode);

		private static readonly SetEntryPointDelegate SetEntryPoint;

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
			if (!(result > 0)) {
				Global.Log.Warning("Could not set entry point for {Method}", mi.Name);	
			}
			//Conditions.Assert(result >0);
		}

		#endregion

		[ClrSymcall(Symbol = "MethodTable::GetSignatureCorElementType", FullyQualified = true)]
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
		[ClrSymcall(Symbol = "MemberLoader::FindField", FullyQualified = true)]
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