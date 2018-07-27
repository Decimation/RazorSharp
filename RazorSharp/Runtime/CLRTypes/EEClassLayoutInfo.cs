using System;
using System.Runtime.InteropServices;
using RazorCommon;

namespace RazorSharp.Runtime.CLRTypes
{

	using UINT32 = UInt32;
	using BYTE = Byte;
	using UINT = UInt32; //probably, maybe

	[StructLayout(LayoutKind.Explicit)]
	public unsafe struct EEClassLayoutInfo
	{
		// size (in bytes) of fixed portion of NStruct.
		[FieldOffset(0)] UINT32 m_cbNativeSize;
		[FieldOffset(4)] UINT32 m_cbManagedSize;

		// 1,2,4 or 8: this is equal to the largest of the alignment requirements
		// of each of the EEClass's members. If the NStruct extends another NStruct,
		// the base NStruct is treated as the first member for the purpose of
		// this calculation.
		[FieldOffset(8)] BYTE m_LargestAlignmentRequirementOfAllMembers;

		// Post V1.0 addition: This is the equivalent of m_LargestAlignmentRequirementOfAllMember
		// for the managed layout.
		[FieldOffset(9)] BYTE m_ManagedLargestAlignmentRequirementOfAllMembers;


		[FieldOffset(10)] BYTE m_bFlags;

		// Packing size in bytes (1, 2, 4, 8 etc.)
		[FieldOffset(11)] BYTE m_cbPackingSize;

		// # of fields that are of the calltime-marshal variety.
		[FieldOffset(12)] UINT m_numCTMFields;

		// An array of FieldMarshaler data blocks, used to drive call-time
		// marshaling of NStruct reference parameters. The number of elements
		// equals m_numCTMFields.
		[FieldOffset(16)] void* m_pFieldMarshalers;

		public LayoutFlags Flags => (LayoutFlags) m_bFlags;

		public bool ZeroSized => Flags.HasFlag(LayoutFlags.ZeroSized);
		public bool IsBlittable => Flags.HasFlag(LayoutFlags.Blittable);

		public override string ToString()
		{
			var table = new ConsoleTable("Field", "Value");
			table.AddRow("Native size", m_cbNativeSize);
			table.AddRow("Managed size", m_cbManagedSize);
			table.AddRow("Largest alignment req of all", m_LargestAlignmentRequirementOfAllMembers);
			table.AddRow("Flags", m_bFlags);
			table.AddRow("Packing size", m_cbPackingSize);
			table.AddRow("CMT fields", m_numCTMFields);
			table.AddRow("Field marshalers", Hex.ToHex(m_pFieldMarshalers));

			table.AddRow("Blittable", IsBlittable);
			table.AddRow("Zero sized", ZeroSized);
			return table.ToMarkDownString();
		}
	}

}