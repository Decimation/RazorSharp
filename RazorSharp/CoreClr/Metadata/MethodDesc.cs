using System.Collections.Generic;
using System.Runtime.InteropServices;
using RazorSharp.CoreClr.Metadata.Enums;
using RazorSharp.Import.Attributes;
using RazorSharp.Import.Enums;
using RazorSharp.Interop;
using RazorSharp.Memory.Pointers;

// ReSharper disable ArrangeAccessorOwnerBody
// ReSharper disable InconsistentNaming
// ReSharper disable UnassignedGetOnlyAutoProperty

namespace RazorSharp.CoreClr.Metadata
{
	[ImportNamespace]
	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct MethodDesc
	{
		static MethodDesc()
		{
			ImportMap = new Dictionary<string, Pointer<byte>>();
		}

		[ImportMap]
		private static readonly Dictionary<string, Pointer<byte>> ImportMap;

		#region Fields

		internal MethodDescFlags3 Flags3AndTokenRemainder { get; }

		internal byte ChunkIndex { get; }

		internal MethodDescFlags2 Flags2 { get; }

		internal ushort SlotNumber { get; }

		internal MethodDescClassification Flags { get; }

		/// <summary>
		///     Valid only if the function is non-virtual,
		///     non-abstract, non-generic (size of this MethodDesc <c>== 16</c>)
		/// </summary>
		internal void* Function { get; }

		#endregion

		#region Accessors

		#region Flags

		internal MethodClassification Classification {
			get { return (MethodClassification) ((ushort) Flags & (ushort) MethodDescClassification.Classification); }
		}

		#endregion

		#endregion

		#region Import

		[ImportCall(ImportCallOptions.Map)]
		internal void Reset()
		{
			fixed (MethodDesc* value = &this) {
				NativeFunctions.CallVoid((void*) ImportMap[nameof(Reset)], value);
			}
		}

		[ImportCall(ImportCallOptions.Map)]
		internal bool IsPointingToNativeCode()
		{
			fixed (MethodDesc* value = &this) {
				return NativeFunctions.Call<bool>((void*) ImportMap[nameof(IsPointingToNativeCode)], value);
			}
		}


		internal void* PreImplementedCode {
			[ImportCall(IdentifierOptions.UseAccessorName, ImportCallOptions.Map)]
			get {
				fixed (MethodDesc* value = &this) {
					return NativeFunctions.CallReturnPointer((void*) ImportMap[nameof(PreImplementedCode)], value);
				}
			}
		}


		internal void* NativeCode {
			[ImportCall(IdentifierOptions.UseAccessorName, ImportCallOptions.Map)]
			get {
				fixed (MethodDesc* value = &this) {
					return NativeFunctions.CallReturnPointer((void*) ImportMap[nameof(NativeCode)], value);
				}
			}
		}

		[ImportCall(ImportCallOptions.Map)]
		internal bool SetNativeCodeInterlocked(long p)
		{
			fixed (MethodDesc* value = &this) {
				return NativeFunctions.Call<bool>((void*) ImportMap[nameof(SetNativeCodeInterlocked)],
				                         value, (void*) p);
			}
		}

		internal int Token {
			[ImportCall("GetMemberDef", ImportCallOptions.Map)]
			get {
				fixed (MethodDesc* value = &this) {
					return NativeFunctions.Call<int>((void*) ImportMap[nameof(Token)], value);
				}
			}
		}


		[ImportCall(ImportCallOptions.Map)]
		internal void* GetILHeader(int fAllowOverrides)
		{
			fixed (MethodDesc* value = &this) {
				return NativeFunctions.CallReturnPointer((void*) ImportMap[nameof(GetILHeader)], value, fAllowOverrides);
			}
		}


		internal long RVA {
			[ImportCall(IdentifierOptions.UseAccessorName, ImportCallOptions.Map)]
			get {
				fixed (MethodDesc* value = &this) {
					return NativeFunctions.Call<long>((void*) ImportMap[nameof(RVA)], value);
				}
			}
		}


		internal MethodTable* MethodTable {
			[ImportCall(IdentifierOptions.UseAccessorName, ImportCallOptions.Map)]
			get {
				fixed (MethodDesc* value = &this) {
					return (MethodTable*) NativeFunctions.CallReturnPointer((void*) ImportMap[nameof(MethodTable)], value);
				}
			}
		}

		#endregion
	}
}