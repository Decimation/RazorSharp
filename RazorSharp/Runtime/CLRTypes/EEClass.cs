#region

using System;
using System.Runtime.InteropServices;
using RazorCommon;
using RazorSharp.Pointers;
using RazorSharp.Utilities;

#endregion

// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable BuiltInTypeReferenceStyle
// ReSharper disable InconsistentNaming

namespace RazorSharp.Runtime.CLRTypes
{

	// class.h

	#region

	using BYTE = Byte;
	using QWORD = UInt64;
	using DWORD = UInt32;
	using WORD = UInt16;

	#endregion

	/// <summary>
	/// Source: https://github.com/dotnet/coreclr/blob/6bb3f84d42b9756c5fa18158db8f724d57796296/src/vm/class.h#L1901<para></para>
	///
	/// DO NOT DEREFERENCE<para></para>
	/// </summary>
	[StructLayout(LayoutKind.Explicit)]
	public unsafe struct EEClass
	{

		#region Fields

		[FieldOffset(0)] private readonly void* m_pGuidInfo;
		[FieldOffset(8)] private readonly void* m_rpOptionalFields;

		//** Status: verified
		[FieldOffset(16)] private readonly MethodTable* m_pMethodTable;

		//** Status: verified
		[FieldOffset(24)] private readonly FieldDesc* m_pFieldDescList;
		[FieldOffset(32)] private readonly void*      m_pChunks;


		//** Status: verified
		[FieldOffset(40)] private readonly uint  m_cbNativeSize;
		[FieldOffset(40)] private readonly void* ohDelegate;
		[FieldOffset(40)] private readonly int   m_ComInterfaceType;
		[FieldOffset(48)] private readonly void* m_pccwTemplate;


		//** Status: verified
		[FieldOffset(56)] private readonly DWORD m_dwAttrClass;

		//** Status: verified
		[FieldOffset(60)] private readonly DWORD m_VMFlags;

		//** Status: verified
		[FieldOffset(64)] private readonly byte m_NormType;

		//** Status: verified
		[FieldOffset(65)] private readonly byte m_fFieldsArePacked;

		/// <summary>
		/// Count of bytes of normal fields of this instance (EEClass,
		/// LayoutEEClass etc.). Doesn't count bytes of "packed" fields
		///
		/// </summary>

		//** Status: verified
		[FieldOffset(66)] private readonly byte m_cbFixedEEClassFields;

		//** Status: verified
		[FieldOffset(67)] private readonly byte m_cbBaseSizePadding;

		#endregion

		#region Accessors

		public MethodTable* MethodTable => m_pMethodTable;

		public bool HasLayout => VMFlags.HasFlag(VMFlags.HasLayout);

		public DWORD Attributes => m_dwAttrClass;

		/// <summary>
		/// Number of bytes to subtract from code:MethodTable::GetBaseSize() to get the actual number of bytes
		/// of instance fields stored in the object on the GC heap.
		/// </summary>
		public byte BaseSizePadding => m_cbBaseSizePadding;

		public VMFlags VMFlags => (VMFlags) m_VMFlags;

		public CorElementType NormalType => (CorElementType) m_NormType;

		/// <summary>
		/// Address-sensitive
		/// </summary>
		private EEClassLayoutInfo* LayoutInfo {
			get {
				//return &((LayoutEEClass *) this)->m_LayoutInfo;
				if (!HasLayout)
					throw new RuntimeException("EEClass does not have LayoutInfo");

				var thisptr = PointerUtils.Add(Unsafe.AddressOf(ref this), sizeof(EEClass));
				return &((LayoutEEClass*) thisptr)->m_LayoutInfo;
			}
		}

		/// <summary>
		/// Abstracted to MethodTable
		///
		/// For use with Runtime.IsBlittable
		/// </summary>
		internal bool IsBlittable => HasLayout && LayoutInfo->IsBlittable;

		/// <summary>
		/// Abstracted to MethodTable
		/// </summary>
		internal int NumInstanceFields => (int) GetPackableField(EEClassFieldId.NumInstanceFields);

		/// <summary>
		/// Abstracted to MethodTable
		/// </summary>
		internal int NumStaticFields => (int) GetPackableField(EEClassFieldId.NumStaticFields);

		/// <summary>
		/// Abstracted to MethodTable
		/// </summary>
		internal int NumMethods => (int) GetPackableField(EEClassFieldId.NumMethods);

		/// <summary>
		/// Abstracted to MethodTable
		/// </summary>
		internal int NumNonVirtualSlots => (int) GetPackableField(EEClassFieldId.NumNonVirtualSlots);

		private DWORD GetPackableField(EEClassFieldId eField)
		{
			//Console.WriteLine(Hex.ToHex(PackedFields));
			return (m_fFieldsArePacked == 1
				? PackedFields->GetPackedField((DWORD) eField)
				: PackedFields->GetUnpackedField((DWORD) eField));
		}

		/// <summary>
		/// Address-sensitive
		/// </summary>
		private PackedDWORDFields* PackedFields =>
			(PackedDWORDFields*) PointerUtils.Add(Unsafe.AddressOf(ref this), m_cbFixedEEClassFields);

		private EEClass* ParentClass => m_pMethodTable->Parent->EEClass;

		internal int FieldDescListLength {
			//There are (m_wNumInstanceFields - GetParentClass()->m_wNumInstanceFields + m_wNumStaticFields) entries
			//get { return (NumInstanceFields - ParentClass->NumInstanceFields + NumStaticFields); }

			get {
				EEClass*     pClass     = m_pMethodTable->EEClass;
				int          fieldCount = pClass->NumInstanceFields + pClass->NumStaticFields;
				MethodTable* pParentMT  = m_pMethodTable->Parent;

				if (pParentMT != null)
					fieldCount -= pParentMT->EEClass->NumInstanceFields;
				return fieldCount;
			}
		}

		/// <summary>
		/// Address-sensitive
		/// </summary>
		internal FieldDesc* FieldDescList {

			get {
				const int fieldDescListFieldOffset = 24;

				//PTR_HOST_MEMBER_TADDR(EEClass, this, m_pFieldDescList)
				var cpy    = (IntPtr) m_pFieldDescList;
				var __this = Unsafe.AddressOf(ref this);
				__this += fieldDescListFieldOffset;
				return (FieldDesc*) PointerUtils.Add(cpy, __this);
			}
		}

		/// <summary>
		/// Address-sensitive
		/// </summary>
		internal MethodDescChunk* MethodDescChunkList {
			//todo: verify
			get {
				const int chunksFieldOffset = 32;
				var       cpy               = (IntPtr) m_pChunks;
				var       __this            = Unsafe.AddressOf(ref this);
				__this += chunksFieldOffset;
				return (MethodDescChunk*) PointerUtils.Add(cpy, __this);
			}
		}

		#endregion

		public override string ToString()
		{
			var table = new ConsoleTable("Field", "Value");
			table.AddRow(nameof(m_pGuidInfo), Hex.ToHex(m_pGuidInfo));
			table.AddRow(nameof(m_rpOptionalFields), Hex.ToHex(m_rpOptionalFields));
			table.AddRow("Method Table", Hex.ToHex(m_pMethodTable));

			//table.AddRow(nameof(m_pChunks), Hex.ToHex(m_pChunks));
			table.AddRow("Native size", m_cbNativeSize);
			table.AddRow(nameof(ohDelegate), Hex.ToHex(ohDelegate));
			table.AddRow(nameof(m_ComInterfaceType), m_ComInterfaceType);
			table.AddRow(nameof(m_pccwTemplate), Hex.ToHex(m_pccwTemplate));
			table.AddRow("Attributes", Hex.ToHex(m_dwAttrClass));
			table.AddRow("Normal type", NormalType);
			table.AddRow("Fields are packed", m_fFieldsArePacked == 1);
			table.AddRow("Fixed EEClass fields", m_cbFixedEEClassFields);
			table.AddRow("Base size padding", m_cbBaseSizePadding);
			table.AddRow("VMFlags", String.Join(", ", VMFlags.GetFlags()));
			table.AddRow("Has layout", HasLayout);


			// !NOTE NOTE NOTE!
			// this->ToString() must be used to view these
			// when the pointer is copied, NumInstanceFields and NumStaticFields
			// read from incorrect memory (not the packed fields like they should)
			/*table.AddRow("Instance fields", NumInstanceFields);
			table.AddRow("Static fields", NumStaticFields);
			table.AddRow("Methods", NumMethods);
			table.AddRow("Non virtual slots", NumNonVirtualSlots);*/


			table.RemoveFromRows(0, "0x0");
			return table.ToMarkDownString();
		}
	}

}