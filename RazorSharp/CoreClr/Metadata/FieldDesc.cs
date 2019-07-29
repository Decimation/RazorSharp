using System.Collections.Generic;
using System.Runtime.InteropServices;
using RazorSharp.CoreClr.Metadata.Enums;
using RazorSharp.Import.Attributes;
using RazorSharp.Import.Enums;
using RazorSharp.Interop;
using RazorSharp.Memory;
using RazorSharp.Memory.Pointers;

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
			ImportMap = new Dictionary<string, Pointer<byte>>();
		}
		
		[ImportMap]
		private static readonly Dictionary<string, Pointer<byte>> ImportMap;

		#region Fields

		private MethodTable* EnclosingMethodTableStub { get; }

		/// <summary>
		///     <para>unsigned m_mb : 24;</para>
		///     <para>unsigned m_isStatic : 1;</para>
		///     <para>unsigned m_isThreadLocal : 1;</para>
		///     <para>unsigned m_isRVA : 1;</para>
		///     <para>unsigned m_prot : 3;</para>
		///     <para>unsigned m_requiresFullMbValue : 1;</para>
		/// </summary>
		internal uint Dword1 { get; }

		/// <summary>
		///     <para>unsigned m_dwOffset : 27;</para>
		///     <para>unsigned m_type : 5;</para>
		/// </summary>
		internal uint Dword2 { get; }

		#endregion

		#region Calculated values

		private bool RequiresFullMBValue => Bits.ReadBit(Dword1, 31);

		internal int Token {
			get {
				var rawToken = (int) (Dword1 & 0xFFFFFF);
				// Check if this FieldDesc is using the packed mb layout
				if (!RequiresFullMBValue)
					return TokenUtil.TokenFromRid(rawToken & (int) MbMask.PackedMbLayoutMbMask,
					                              CorTokenType.FieldDef);

				return TokenUtil.TokenFromRid(rawToken, CorTokenType.FieldDef);
			}
		}

		internal int Offset => (int) (Dword2 & 0x7FFFFFF);

		internal CorElementType CorType => (CorElementType) (int) ((Dword2 >> 27) & 0x7FFFFFF);

		internal ProtectionLevel ProtectionLevel => (ProtectionLevel) (int) ((Dword1 >> 26) & 0x3FFFFFF);

		internal bool IsPointer => CorType == CorElementType.Ptr;


		internal bool IsStatic => Bits.ReadBit(Dword1, 24);


		internal bool IsThreadLocal => Bits.ReadBit(Dword1, 25);


		internal bool IsRVA => Bits.ReadBit(Dword1, 26);

		#endregion

		#region Imports

		[ImportCall(CallOptions = ImportCallOptions.Map)]
		internal int LoadSize()
		{
			fixed (FieldDesc* value = &this) {
				return NativeFunctions.Call<int>((void*) ImportMap[nameof(LoadSize)], value);
			}
		}

		[ImportCall(CallOptions = ImportCallOptions.Map)]
		internal void* GetCurrentStaticAddress()
		{
			fixed (FieldDesc* value = &this) {
				return NativeFunctions.CallReturnPointer((void*) ImportMap[nameof(GetCurrentStaticAddress)], value);
			}
		}

		
		
		[ImportCall(CallOptions = ImportCallOptions.Map)]
		internal void* GetApproxEnclosingMethodTable()
		{
			fixed (FieldDesc* value = &this) {
				return NativeFunctions.CallReturnPointer((void*) ImportMap[nameof(GetApproxEnclosingMethodTable)],
				                                value);
			}
		}

		#endregion
	}
}