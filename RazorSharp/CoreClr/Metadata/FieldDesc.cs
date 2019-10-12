using System.Collections.Generic;
using System.Runtime.InteropServices;
using RazorSharp.Core;
using RazorSharp.CoreClr.Metadata.Enums;
using RazorSharp.Import;
using RazorSharp.Import.Attributes;
using RazorSharp.Import.Enums;
using RazorSharp.Interop;
using RazorSharp.Memory;
using RazorSharp.Memory.Pointers;
using RazorSharp.Utilities;

// ReSharper disable UnassignedGetOnlyAutoProperty
// ReSharper disable InconsistentNaming

namespace RazorSharp.CoreClr.Metadata
{
	[ImportNamespace]
	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct FieldDesc
	{
		static FieldDesc()
		{
			ImportManager.Value.Load(typeof(FieldDesc), Clr.Value.Imports);
		}
		
		[ImportMapDesignation]
		private static readonly ImportMap Imports = new ImportMap();

		#region Fields

		private MethodTable* EnclosingMethodTableStub { get; }

		/// <summary>
		/// <c>DWORD</c> #1
		///     <para>unsigned m_mb : 24;</para>
		///     <para>unsigned m_isStatic : 1;</para>
		///     <para>unsigned m_isThreadLocal : 1;</para>
		///     <para>unsigned m_isRVA : 1;</para>
		///     <para>unsigned m_prot : 3;</para>
		///     <para>unsigned m_requiresFullMbValue : 1;</para>
		/// </summary>
		private uint UInt1 { get; }

		/// <summary>
		/// <c>DWORD</c> #2
		///     <para>unsigned m_dwOffset : 27;</para>
		///     <para>unsigned m_type : 5;</para>
		/// </summary>
		private uint UInt2 { get; }

		#endregion

		#region Calculated values

		private bool RequiresFullMBValue => Bits.ReadBit(UInt1, 31);

		internal int Token {
			get {
				var rawToken = (int) (UInt1 & 0xFFFFFF);
				// Check if this FieldDesc is using the packed mb layout
				if (!RequiresFullMBValue)
					return Tokens.TokenFromRid(rawToken & (int) MbMask.PackedMbLayoutMbMask,
					                              CorTokenType.FieldDef);

				return Tokens.TokenFromRid(rawToken, CorTokenType.FieldDef);
			}
		}

		internal int Offset => (int) (UInt2 & 0x7FFFFFF);

		internal CorElementType CorType => (CorElementType) (int) ((UInt2 >> 27) & 0x7FFFFFF);

		internal ProtectionLevel ProtectionLevel => (ProtectionLevel) (int) ((UInt1 >> 26) & 0x3FFFFFF);

		internal bool IsPointer => CorType == CorElementType.Ptr;


		internal bool IsStatic => Bits.ReadBit(UInt1, 24);


		internal bool IsThreadLocal => Bits.ReadBit(UInt1, 25);


		internal bool IsRVA => Bits.ReadBit(UInt1, 26);

		#endregion

		#region Imports

		[ImportCall(CallOptions = ImportCallOptions.Map)]
		internal int LoadSize()
		{
			fixed (FieldDesc* value = &this) {
				return Functions.Native.Call<int>((void*) Imports[nameof(LoadSize)], value);
			}
		}

		[ImportCall(CallOptions = ImportCallOptions.Map)]
		internal void* GetCurrentStaticAddress()
		{
			fixed (FieldDesc* value = &this) {
				return Functions.Native.CallReturnPointer((void*) Imports[nameof(GetCurrentStaticAddress)], value);
			}
		}

		
		
		[ImportCall(CallOptions = ImportCallOptions.Map)]
		internal void* GetApproxEnclosingMethodTable()
		{
			fixed (FieldDesc* value = &this) {
				return Functions.Native.CallReturnPointer((void*) Imports[nameof(GetApproxEnclosingMethodTable)],
				                                value);
			}
		}

		#endregion
	}
}