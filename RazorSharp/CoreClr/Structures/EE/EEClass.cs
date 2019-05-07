#region

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using SimpleSharp;
using SimpleSharp.Diagnostics;
using SimpleSharp.Extensions;
using SimpleSharp.Strings;
using RazorSharp.CoreClr.Meta;
using RazorSharp.CoreClr.Structures.Enums;
using RazorSharp.Memory;
using RazorSharp.Memory.Pointers;

// ReSharper disable UnusedMember.Local

// ReSharper disable FieldCanBeMadeReadOnly.Local

#endregion

// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable BuiltInTypeReferenceStyle
// ReSharper disable InconsistentNaming

namespace RazorSharp.CoreClr.Structures.EE
{
	#region

	using DWORD = UInt32;

	#endregion

	/// <summary>
	///     <para>
	///         CLR <see cref="EEClass" />. Functionality is implemented in this <c>struct</c> and exposed via
	///         <see cref="MetaType" />
	///     </para>
	///     <para>Corresponding files:</para>
	///     <list type="bullet">
	///         <item>
	///             <description>/src/vm/class.h</description>
	///         </item>
	///         <item>
	///             <description>/src/vm/class.cpp</description>
	///         </item>
	///         <item>
	///             <description>/src/vm/class.inl</description>
	///         </item>
	///     </list>
	///     <para>Lines of interest:</para>
	///     <list type="bullet">
	///         <item>
	///             <description>/src/vm/class.h: 1901</description>
	///         </item>
	///     </list>
	///     <remarks>
	///         This should only be accessed via <see cref="Pointer{T}" />
	///     </remarks>
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct EEClass
	{
		#region Fields

		private void* m_pGuidInfo;

		private void* m_rpOptionalFields;

		private MethodTable* m_pMethodTable;

		private FieldDesc* m_pFieldDescList;

		private void* m_pChunks;


		#region Union 1

		/// <summary>
		///     <para>Union 1</para>
		///     <para>void* <see cref="m_ohDelegate" /></para>
		///     <para>uint <see cref="m_cbNativeSize" /></para>
		///     <para>int <see cref="m_ComInterfaceType" /></para>
		/// </summary>
		private void* m_union1;

		private void* m_ohDelegate => m_union1;

		private uint m_cbNativeSize {
			get {
				fixed (EEClass* value = &this) {
					Pointer<uint> ptr = &value->m_union1;
					return ptr.Reference;
				}
			}
		}

		private int m_ComInterfaceType {
			get {
				fixed (EEClass* value = &this) {
					Pointer<int> ptr = &value->m_union1;
					return ptr.Reference;
				}
			}
		}

		#endregion


		private void* m_pccwTemplate;

		private DWORD m_dwAttrClass;

		private DWORD m_VMFlags;

		private byte m_NormType;

		private byte m_fFieldsArePacked;

		private byte m_cbFixedEEClassFields;

		private byte m_cbBaseSizePadding;

		#endregion

		#region Accessors

		public int ComInterfaceType => m_ComInterfaceType;

		/// <summary>
		///     Count of bytes of normal fields of this instance (<see cref="EEClass" />,
		///     <see cref="LayoutEEClass" /> etc.). Doesn't count bytes of "packed" fields
		/// </summary>
		internal byte FixedEEClassFields => m_cbFixedEEClassFields;

		/// <summary>
		///     Corresponding <see cref="MethodTable" /> of this <see cref="EEClass" />
		/// </summary>
		internal Pointer<MethodTable> MethodTable => m_pMethodTable;


		/// <summary>
		///     Whether this <see cref="EEClass" /> has a <see cref="EEClassLayoutInfo" />
		/// </summary>
		internal bool HasLayout => VMFlags.HasFlag(VMFlags.HasLayout);

		/// <summary>
		///     <see cref="DWORD" /> of <see cref="TypeAttributes" />
		///     <remarks>
		///         Equal to WinDbg's <c>!DumpClass</c> <c>"Class Attributes"</c> value in hexadecimal format.
		///     </remarks>
		/// </summary>
		private DWORD Attributes => m_dwAttrClass;

		/// <summary>
		///     <remarks>
		///         Equal to <see cref="Type.Attributes" />
		///     </remarks>
		/// </summary>
		internal TypeAttributes TypeAttributes => (TypeAttributes) Attributes;

		/// <summary>
		///     Number of bytes to subtract from <see cref="Structures.MethodTable.BaseSize" /> to get the actual number of bytes
		///     of instance fields stored in the object on the GC heap.
		/// </summary>
		internal byte BaseSizePadding => m_cbBaseSizePadding;

		/// <summary>
		///     <para>Size of fixed portion in bytes </para>
		///     <para>Valid only if <see cref="IsBlittable" /> or <see cref="HasLayout" /> is true; 0 otherwise</para>
		///     <remarks>
		///         <para>Equal to (<see cref="EEClassLayoutInfo" />) <see cref="EEClassLayoutInfo.NativeSize" /> </para>
		///         <para>Abstracted to <see cref="Unsafe.NativeSizeOf{T}" /></para>
		///     </remarks>
		/// </summary>
		internal int NativeSize => (int) m_cbNativeSize;

		internal VMFlags VMFlags => (VMFlags) m_VMFlags;

		internal CorElementType NormalType => (CorElementType) m_NormType;

		/// <summary>
		///     <remarks>
		///         Address-sensitive
		///     </remarks>
		/// </summary>
		internal Pointer<EEClassLayoutInfo> LayoutInfo {
			get {
				//return &((LayoutEEClass *) this)->m_LayoutInfo;
				Conditions.Assert(HasLayout, "EEClass does not have LayoutInfo");

				var thisptr = Unsafe.AddressOf(ref this)
				                    .Add(sizeof(EEClass))
				                    .Address;

				// ReSharper disable once ArrangeRedundantParentheses
				return &((LayoutEEClass*) thisptr)->m_LayoutInfo;
			}
		}

		/// <summary>
		///     Abstracted to <see cref="MethodTable"/>
		///     <remarks>
		///         For use with <see cref="Runtime.IsBlittable{T}" />
		///     </remarks>
		/// </summary>
		internal bool IsBlittable => HasLayout && LayoutInfo.Reference.IsBlittable;

		/// <summary>
		///     Abstracted to <see cref="MethodTable"/>
		/// </summary>
		internal int NumInstanceFields => (int) GetPackableField(EEClassFieldId.NumInstanceFields);

		/// <summary>
		///     Abstracted to <see cref="MethodTable"/>
		/// </summary>
		internal int NumStaticFields => (int) GetPackableField(EEClassFieldId.NumStaticFields);

		/// <summary>
		///     Abstracted to <see cref="MethodTable"/>
		/// </summary>
		internal int NumMethods => (int) GetPackableField(EEClassFieldId.NumMethods);

		/// <summary>
		///     Abstracted to <see cref="MethodTable"/>
		/// </summary>
		internal int NumNonVirtualSlots => (int) GetPackableField(EEClassFieldId.NumNonVirtualSlots);

		private DWORD GetPackableField(EEClassFieldId eField)
		{
			return m_fFieldsArePacked == 1
				? PackedFields->GetPackedField((DWORD) eField)
				: PackedFields->GetUnpackedField((DWORD) eField);
		}

		/// <summary>
		///     <remarks>
		///         Address-sensitive
		///     </remarks>
		/// </summary>
		private PackedDWORDFields* PackedFields =>
			(PackedDWORDFields*) Unsafe.AddressOf(ref this).Add(m_cbFixedEEClassFields);

		private Pointer<EEClass> ParentClass => m_pMethodTable->Parent.Reference.EEClass;

		/// <summary>
		///     Abstracted to <see cref="MethodTable"/>
		/// </summary>
		internal int FieldDescListLength {
			//There are (m_wNumInstanceFields - GetParentClass()->m_wNumInstanceFields + m_wNumStaticFields) entries
			//get { return (NumInstanceFields - ParentClass->NumInstanceFields + NumStaticFields); }

			get {
				Pointer<EEClass>     pClass     = m_pMethodTable->EEClass;
				int                  fieldCount = pClass.Reference.NumInstanceFields + pClass.Reference.NumStaticFields;
				Pointer<MethodTable> pParentMT  = m_pMethodTable->Parent;

				if (!pParentMT.IsNull)
					fieldCount -= pParentMT.Reference.EEClass.Reference.NumInstanceFields;

				return fieldCount;
			}
		}

		/// <summary>
		///     <remarks>
		///         Address-sensitive
		///     </remarks>
		/// </summary>
		internal Pointer<FieldDesc> FieldDescList {
			get {
				//PTR_HOST_MEMBER_TADDR(EEClass, this, m_pFieldDescList)
				return Runtime.PTR_HOST_MEMBER_TADDR(ref this,
				                                     FIELD_DESC_LIST_FIELD_OFFSET, m_pFieldDescList);
			}
		}


		/// <summary>
		///     <remarks>
		///         Address-sensitive
		///     </remarks>
		/// </summary>
		internal Pointer<MethodDescChunk> MethodDescChunkList {
			//todo: verify
			get { return Runtime.PTR_HOST_MEMBER_TADDR(ref this, CHUNKS_FIELD_OFFSET, m_pChunks); }
		}

		#endregion

		#region EEClass offsets

		/// <summary>
		///     Offset for the field <see cref="EEClass.m_pFieldDescList" />
		///     <remarks>
		///         Relative to address of a <see cref="EEClass" />
		///     </remarks>
		/// </summary>
		private const int FIELD_DESC_LIST_FIELD_OFFSET = 24;

		/// <summary>
		///     Offset for the field <see cref="EEClass.m_pChunks" />
		///     <remarks>
		///         Relative to address of a <see cref="EEClass" />
		///     </remarks>
		/// </summary>
		private const int CHUNKS_FIELD_OFFSET = 32;

		#endregion

		public override string ToString()
		{
			var table = new ConsoleTable("Field", "Value");

//			table.AddRow(nameof(m_pGuidInfo), Hex.ToHex(m_pGuidInfo));
//			table.AddRow(nameof(m_rpOptionalFields), Hex.ToHex(m_rpOptionalFields));
			table.AddRow("Method Table", Hex.ToHex(m_pMethodTable));

//			table.AddRow(nameof(m_pChunks), Hex.ToHex(m_pChunks));
			table.AddRow("Native size", m_cbNativeSize);

//			table.AddRow(nameof(ohDelegate), Hex.ToHex(ohDelegate));
//			table.AddRow(nameof(m_ComInterfaceType), m_ComInterfaceType);
//			table.AddRow(nameof(m_pccwTemplate), Hex.ToHex(m_pccwTemplate));
			table.AddRow("Attributes", String.Format("{0} ({1})", Hex.ToHex(m_dwAttrClass), TypeAttributes));
			table.AddRow("Normal type", NormalType);
			table.AddRow("Fields are packed", m_fFieldsArePacked == 1);
			table.AddRow("Fixed EEClass fields", m_cbFixedEEClassFields);
			table.AddRow("Base size padding", m_cbBaseSizePadding);
			table.AddRow("VMFlags", VMFlags.JoinFlags());
			table.AddRow("Has layout", HasLayout);


			// !NOTE NOTE NOTE!
			// this->ToString() must be used to view these
			// when the pointer is copied, NumInstanceFields and NumStaticFields
			// read from incorrect memory (not the packed fields like they should)
			/*table.AddRow("Instance fields", NumInstanceFields);
			table.AddRow("Static fields", NumStaticFields);
			table.AddRow("Methods", NumMethods);
			table.AddRow("Non virtual slots", NumNonVirtualSlots);*/


//			table.RemoveFromRows(0, "0x0");
			return table.ToString();
		}
	}
}