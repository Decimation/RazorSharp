using System;
using System.Linq;
using System.Runtime.InteropServices;
using RazorCommon;
// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable BuiltInTypeReferenceStyle
// ReSharper disable InconsistentNaming

namespace RazorSharp.Runtime.CLRTypes
{

	// class.h
	using BYTE = Byte;
	using QWORD = UInt64;
	using DWORD = UInt32;
	using WORD = UInt16;

	/// <summary>
	/// Source: https://github.com/dotnet/coreclr/blob/6bb3f84d42b9756c5fa18158db8f724d57796296/src/vm/class.h#L1901
	/// </summary>
	[StructLayout(LayoutKind.Explicit)]
	public unsafe struct EEClass
	{

		#region Fields

		[FieldOffset(0)] private readonly void* m_pGuidInfo;
		[FieldOffset(8)] private readonly void* m_rpOptionalFields;

		//** Status: verified
		[FieldOffset(16)] private readonly void*      m_pMethodTable;
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

		/*
		 * Number of bytes to subtract from code:MethodTable::GetBaseSize() to get the actual number of bytes
		 * of instance fields stored in the object on the GC heap.
		 */
		//** Status: verified
		[FieldOffset(67)] private readonly byte m_cbBaseSizePadding;

		#endregion

		#region Accessors

		public bool HasLayout => VMFlags.HasFlag(VMFlags.HasLayout);

		public DWORD Attributes => m_dwAttrClass;

		public byte BaseSizePadding => m_cbBaseSizePadding;

		public VMFlags VMFlags => (VMFlags) m_VMFlags;

		public CorElementType NormalType => (CorElementType) m_NormType;

		private EEClassLayoutInfo* LayoutInfo {
			get {
				//return &((LayoutEEClass *) this)->m_LayoutInfo;
				if (!HasLayout) return null;

				var thisptr = Unsafe.AddressOf(ref this);
				thisptr += sizeof(EEClass);
				return &((LayoutEEClass*) thisptr)->m_LayoutInfo;
			}
		}

		/// <summary>
		/// For use with Runtime.IsBlittable
		/// </summary>
		internal bool IsBlittable {
			get { return HasLayout && LayoutInfo->IsBlittable; }
		}

		#endregion

		public override string ToString()
		{
			var table = new ConsoleTable("Field", "Value");
			table.AddRow(nameof(m_pGuidInfo), Hex.ToHex(m_pGuidInfo));
			table.AddRow(nameof(m_rpOptionalFields), Hex.ToHex(m_rpOptionalFields));
			table.AddRow("Method Table", Hex.ToHex(m_pMethodTable));
			table.AddRow(nameof(m_pFieldDescList), Hex.ToHex(m_pFieldDescList));
			table.AddRow(nameof(m_pChunks), Hex.ToHex(m_pChunks));
			table.AddRow("Native size", m_cbNativeSize);
			table.AddRow(nameof(ohDelegate), Hex.ToHex(ohDelegate));
			table.AddRow(nameof(m_ComInterfaceType), m_ComInterfaceType);
			table.AddRow(nameof(m_pccwTemplate), Hex.ToHex(m_pccwTemplate));
			table.AddRow("Attributes", Hex.ToHex(m_dwAttrClass));
			table.AddRow("Normal type", NormalType);
			table.AddRow("Fields are packed", m_fFieldsArePacked);
			table.AddRow("Fixed EEClass fields", m_cbFixedEEClassFields);
			table.AddRow("Base size padding", m_cbBaseSizePadding);
			table.AddRow("VMFlags", String.Join(", ", VMFlags.GetFlags()));


			table.RemoveEmptyRows();
			return table.ToMarkDownString();
		}
	}

}