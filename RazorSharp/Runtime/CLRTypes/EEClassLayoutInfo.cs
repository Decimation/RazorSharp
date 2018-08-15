#region

using System;
using System.Runtime.InteropServices;
using RazorCommon;

// ReSharper disable ConvertToAutoPropertyWhenPossible

#endregion

// ReSharper disable InconsistentNaming
// ReSharper disable BuiltInTypeReferenceStyle

namespace RazorSharp.Runtime.CLRTypes
{

	#region

	using UINT32 = UInt32;
	using BYTE = Byte;
	using UINT = UInt32;

	#endregion

//probably, maybe


	[StructLayout(LayoutKind.Explicit)]
	internal unsafe struct EEClassLayoutInfo
	{
		// why is there an m_cbNativeSize in EEClassLayoutInfo and EEClass?
		[FieldOffset(0)] private readonly UINT32 m_cbNativeSize;
		[FieldOffset(4)] private readonly UINT32 m_cbManagedSize;

		// 1,2,4 or 8: this is equal to the largest of the alignment requirements
		// of each of the EEClass's members. If the NStruct extends another NStruct,
		// the base NStruct is treated as the first member for the purpose of
		// this calculation.
		[FieldOffset(8)] private readonly BYTE m_LargestAlignmentRequirementOfAllMembers;

		// Post V1.0 addition: This is the equivalent of m_LargestAlignmentRequirementOfAllMember
		// for the managed layout.
		[FieldOffset(9)] private readonly BYTE m_ManagedLargestAlignmentRequirementOfAllMembers;

		[FieldOffset(10)] private readonly BYTE m_bFlags;

		// Packing size in bytes (1, 2, 4, 8 etc.)
		[FieldOffset(11)] private readonly BYTE m_cbPackingSize;

		// # of fields that are of the calltime-marshal variety.
		[FieldOffset(12)] private readonly UINT m_numCTMFields;


		[FieldOffset(16)] private readonly void* m_pFieldMarshalers;

		/// <summary>
		/// <para>Size (in bytes) of fixed portion of NStruct.</para>
		///
		/// <para>Equal to: Marshal.SizeOf and EEClass.m_cbNativeSize</para>
		/// </summary>
		internal uint NativeSize => m_cbNativeSize;

		/// <summary>
		/// Equal to: Unsafe.SizeOf
		/// </summary>
		internal uint ManagedSize => m_cbManagedSize;

		public LayoutFlags Flags => (LayoutFlags) m_bFlags;


		public bool ZeroSized => Flags.HasFlag(LayoutFlags.ZeroSized);

		public bool IsBlittable => Flags.HasFlag(LayoutFlags.Blittable);

		public override string ToString()
		{
			var table = new ConsoleTable("Field", "Value");
			table.AddRow("Native size", m_cbNativeSize);
			table.AddRow("Managed size", m_cbManagedSize);
			table.AddRow("Largest alignment req of all", m_LargestAlignmentRequirementOfAllMembers);
			table.AddRow("Flags", String.Format("{0} ({1})", m_bFlags, String.Join(", ", Flags.GetFlags())));

			table.AddRow("Packing size", m_cbPackingSize);
			table.AddRow("CTM fields", m_numCTMFields);
			table.AddRow("Field marshalers", Hex.ToHex(m_pFieldMarshalers));
			table.AddRow("Blittable", IsBlittable);
			table.AddRow("Zero sized", ZeroSized);

			return table.ToMarkDownString();
		}
	}

}