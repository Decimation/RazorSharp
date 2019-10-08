#region

using System;
using System.Runtime.InteropServices;
using RazorSharp.Core;
using RazorSharp.CoreClr.Metadata.Enums;
using RazorSharp.CoreClr.Metadata.ExecutionEngine;
using RazorSharp.Import;
using RazorSharp.Import.Attributes;
using RazorSharp.Memory.Pointers;

#endregion

// ReSharper disable UnassignedGetOnlyAutoProperty
// ReSharper disable InconsistentNaming

namespace RazorSharp.CoreClr.Metadata
{
	/// <summary>
	///     The value of lowest two bits describe what the union contains
	///     <remarks>
	///         Use with <see cref="Metadata.MethodTable.UnionType" />
	///     </remarks>
	/// </summary>
	[Flags]
	public enum UnionType
	{
		/// <summary>
		///     0 - pointer to <see cref="EEClass" />
		///     This <see cref="MethodTable" /> is the canonical method table.
		/// </summary>
		EEClass = 0,

		/// <summary>
		///     1 - not used
		/// </summary>
		Invalid = 1,

		/// <summary>
		///     2 - pointer to canonical <see cref="MethodTable" />.
		/// </summary>
		MethodTable = 2,

		/// <summary>
		///     3 - pointer to indirection cell that points to canonical <see cref="MethodTable" />.
		///     (used only if FEATURE_PREJIT is defined)
		/// </summary>
		Indirection = 3
	}

	[ImportNamespace]
	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct MethodTable
	{
		static MethodTable()
		{
			ImportManager.Value.Load(typeof(MethodTable), Clr.Value.Imports);
		}
		
		internal short               ComponentSize { get; }
		internal MethodTableFlagsLow FlagsLow      { get; }
		internal int                 BaseSize      { get; }
		internal MethodTableFlags2   Flags2        { get; }
		internal short               RawToken      { get; }
		internal short               NumVirtuals   { get; }
		internal short               NumInterfaces { get; }
		internal void*               Parent        { get; }
		internal void*               Module        { get; }
		internal void*               WriteableData { get; }

		internal MethodTableFlags Flags {
			get {
				fixed (MethodTable* ptr = &this) {
					return (MethodTableFlags) (*(int*) ptr);
				}
			}
		}

		#region Union 1

		/// <summary>
		///     <para>Union 1</para>
		///     <para>EEClass* <see cref="EEClass" /></para>
		///     <para>MethodTable* <see cref="Canon" /></para>
		/// </summary>
		private void* Union1 { get; }

		internal Pointer<EEClass>     EEClass => (EEClass*) Union1;
		internal Pointer<MethodTable> Canon   => (MethodTable*) Union1;

		#endregion

		#region Union 2

		/// <summary>
		///     <para>Union 2</para>
		///     <para>void* <see cref="PerInstInfo" /></para>
		///     <para>void* <see cref="ElementTypeHandle" /></para>
		///     <para>void* <see cref="MultipurposeSlot1" /></para>
		/// </summary>
		private void* Union2 { get; }

		internal Pointer<byte> PerInstInfo => Union2;

		internal Pointer<byte> ElementTypeHandle => Union2;

		internal Pointer<byte> MultipurposeSlot1 => Union2;

		#endregion

		#region Union 3

		/// <summary>
		///     <para>Union 3</para>
		///     <para>void* <see cref="InterfaceMap" /></para>
		///     <para>void* <see cref="MultipurposeSlot2" /></para>
		/// </summary>
		private void* Union3 { get; }

		internal Pointer<byte> InterfaceMap => Union3;

		internal Pointer<byte> MultipurposeSlot2 => Union3;

		#endregion

		/// <summary>
		///     Bit mask for <see cref="UnionType" />
		/// </summary>
		private const long UNION_MASK = 3;

		/// <summary>
		///     Describes what the union at offset <c>40</c> (<see cref="Union1" />)
		///     contains.
		/// </summary>
		internal UnionType UnionType {
			get {
				long l = (long) Union1;
				return (UnionType) (l & UNION_MASK);
			}
		}
	}
}