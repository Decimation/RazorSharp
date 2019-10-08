using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using RazorSharp.Core;
using RazorSharp.CoreClr.Metadata.Enums;
using RazorSharp.Import;
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
			ImportManager.Value.Load(typeof(MethodDesc), Clr.Value.Imports);
		}

		[ImportMapDesignation]
		private static readonly ImportMap Imports = new ImportMap();

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
				Functions.Native.CallVoid((void*) Imports[nameof(Reset)], value);
			}
		}

		[ImportCall(ImportCallOptions.Map)]
		internal bool IsPointingToNativeCode()
		{
			fixed (MethodDesc* value = &this) {
				return Functions.Native.Call<bool>((void*) Imports[nameof(IsPointingToNativeCode)], value);
			}
		}


		internal void* PreImplementedCode {
			[ImportCall(IdentifierOptions.UseAccessorName, ImportCallOptions.Map)]
			get {
				fixed (MethodDesc* value = &this) {
					return Functions.Native.CallReturnPointer((void*) Imports[nameof(PreImplementedCode)], value);
				}
			}
		}


		internal void* NativeCode {
			[ImportCall(IdentifierOptions.UseAccessorName, ImportCallOptions.Map)]
			get {
				fixed (MethodDesc* value = &this) {
					return Functions.Native.CallReturnPointer((void*) Imports[nameof(NativeCode)], value);
				}
			}
		}

		[ImportCall(ImportCallOptions.Map)]
		internal bool SetNativeCodeInterlocked(long p)
		{
			fixed (MethodDesc* value = &this) {
				return Functions.Native.Call<bool>((void*) Imports[nameof(SetNativeCodeInterlocked)],
				                         value, (void*) p);
			}
		}

		internal int Token {
			[ImportCall("GetMemberDef", ImportCallOptions.Map)]
			get {
				fixed (MethodDesc* value = &this) {
					return Functions.Native.Call<int>((void*) Imports[nameof(Token)], value);
				}
			}
		}


		[Obsolete]
		[ImportCall(ImportCallOptions.Map)]
		internal void* GetILHeader(int fAllowOverrides)
		{
			fixed (MethodDesc* value = &this) {
				return Functions.Native.CallReturnPointer((void*) Imports[nameof(GetILHeader)], value, fAllowOverrides);
			}
		}


		internal long RVA {
			[ImportCall(IdentifierOptions.UseAccessorName, ImportCallOptions.Map)]
			get {
				fixed (MethodDesc* value = &this) {
					return Functions.Native.Call<long>((void*) Imports[nameof(RVA)], value);
				}
			}
		}


		internal MethodTable* MethodTable {
			[ImportCall(IdentifierOptions.UseAccessorName, ImportCallOptions.Map)]
			get {
				fixed (MethodDesc* value = &this) {
					return (MethodTable*) Functions.Native.CallReturnPointer((void*) Imports[nameof(MethodTable)], value);
				}
			}
		}

		#endregion
	}
}