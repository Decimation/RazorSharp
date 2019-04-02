#region

using System;
using System.Runtime.InteropServices;
using RazorSharp.CoreClr.Structures;
using RazorSharp.Memory;
using RazorSharp.Memory.Calling;
using RazorSharp.Memory.Calling.Symbols;
using RazorSharp.Memory.Calling.Symbols.Attributes;
using RazorSharp.Pointers;

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
			Symcall.BindQuick(typeof(ClrFunctions));
			JIT_GetRuntimeType = Clr.GetClrFunction<GetRuntimeType>(nameof(JIT_GetRuntimeType));
		}

		private delegate void* GetRuntimeType(MethodTable* value);

		private static readonly GetRuntimeType JIT_GetRuntimeType;

		/// <summary>
		///     Returns the corresponding <see cref="Type" /> for a <see cref="MethodTable" /> pointer.
		/// </summary>
		/// <param name="value"><see cref="MethodTable" /> pointer</param>
		/// <returns>A pointer to a <see cref="Type"/> object</returns>
		/// <exception cref="NativeCallException">Method has not been bound</exception>
		internal static Type JIT_GetRuntimeType_Safe(MethodTable* value)
		{
			void* ptr = JIT_GetRuntimeType(value);
			return Mem.Read<Type>(&ptr);
		}


		[ClrSymcall(UseMethodNameOnly = true)]
		internal static Pointer<byte> JIT_GetStaticFieldAddr_Context(FieldDesc* value)
		{
			throw new NativeCallException();
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
			var cSig   = pMT.Reference.GetSignatureCorElementType();
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
			throw new NativeCallException();
		}

		#endregion
	}
}