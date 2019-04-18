#region

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using RazorSharp.CoreClr.Structures;
using RazorSharp.Memory.Extern;
using RazorSharp.Memory.Extern.Symbols;
using RazorSharp.Memory.Extern.Symbols.Attributes;
using RazorSharp.Memory.Pointers;

// ReSharper disable IdentifierTypo

#endregion

// ReSharper disable InconsistentNaming

namespace RazorSharp.CoreClr
{
	#region

	using CSUnsafe = Unsafe;

	#endregion


	/// <summary>
	///     Provides an interface for interacting with and calling native CLR functions
	///     <remarks>
	///         All GC-related functions are WKS, not SVR
	///     </remarks>
	/// </summary>
	[ClrSymNamespace]
	internal static unsafe class ClrFunctions
	{
		private static readonly GetRuntimeType JIT_GetRuntimeType;

		static ClrFunctions()
		{
			Symload.Load(typeof(ClrFunctions));
			JIT_GetRuntimeType = Runtime.GetClrFunction<GetRuntimeType>(nameof(JIT_GetRuntimeType));
		}

		/// <summary>
		///     Returns the corresponding <see cref="Type" /> for a <see cref="MethodTable" /> pointer.
		/// </summary>
		/// <param name="value"><see cref="MethodTable" /> pointer</param>
		/// <returns>A pointer to a <see cref="Type" /> object</returns>
		/// <exception cref="NativeCallException">Method has not been bound</exception>
		internal static Type JIT_GetRuntimeType_Safe(MethodTable* value)
		{
			void* ptr = JIT_GetRuntimeType(value);
			return CSUnsafe.Read<Type>(&ptr);
		}


		[Symcall(UseMethodNameOnly = true)]
		internal static Pointer<byte> JIT_GetStaticFieldAddr_Context(FieldDesc* value)
		{
			throw new NativeCallException();
		}

		private delegate void* GetRuntimeType(MethodTable* value);


		#region FieldField

		internal static Pointer<FieldDesc> FindField(Type t, string name) => FindField(t.GetMethodTable(), name);

		internal static Pointer<FieldDesc> FindField(Pointer<MethodTable> pMT, string name)
		{
			Pointer<byte>      module = pMT.Reference.Module;
			var                pStr   = Marshal.StringToHGlobalAnsi(name);
			uint               cSig   = pMT.Reference.GetSignatureCorElementType();
			Pointer<FieldDesc> field  = FindField(pMT, pStr, IntPtr.Zero, cSig, module, 0);
			Marshal.FreeHGlobal(pStr);
			return field;
		}


		[Symcall(Symbol = "MemberLoader::FindField", FullyQualified = true)]
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