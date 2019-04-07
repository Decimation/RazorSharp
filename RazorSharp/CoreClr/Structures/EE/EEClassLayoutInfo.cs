#region

using System;
using System.Runtime.InteropServices;
using RazorCommon;
using RazorCommon.Utilities;
using RazorSharp.CoreClr.Meta;
using RazorSharp.Memory;

// ReSharper disable FieldCanBeMadeReadOnly.Local

#endregion

// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable InconsistentNaming
// ReSharper disable BuiltInTypeReferenceStyle

namespace RazorSharp.CoreClr.Structures.EE
{
	#region

	using UINT32 = UInt32;
	using BYTE = Byte;
	using UINT = UInt32;

	#endregion


	/// <summary>
	///     <para>
	///         CLR <see cref="EEClassLayoutInfo" />. Functionality is implemented in this <c>struct</c> and exposed via
	///         <see cref="MetaType" />
	///     </para>
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct EEClassLayoutInfo
	{
		#region Fields

		// why is there an m_cbNativeSize in EEClassLayoutInfo and EEClass?
		private UINT32 m_cbNativeSize;


		private UINT32 m_cbManagedSize;

		// 1,2,4 or 8: this is equal to the largest of the alignment requirements
		// of each of the EEClass's members. If the NStruct extends another NStruct,
		// the base NStruct is treated as the first member for the purpose of
		// this calculation.
		private BYTE m_LargestAlignmentRequirementOfAllMembers;

		// Post V1.0 addition: This is the equivalent of m_LargestAlignmentRequirementOfAllMember
		// for the managed layout.
		private BYTE m_ManagedLargestAlignmentRequirementOfAllMembers;

		private BYTE m_bFlags;

		private BYTE m_cbPackingSize;

		private void* m_pFieldMarshalers;

		#endregion

		/// <summary>
		///     Packing size in bytes (1, 2, 4, 8 etc.)
		/// </summary>
		internal BYTE PackingSize => m_cbPackingSize;

		/// <summary>
		///     # of fields that are of the calltime-marshal variety.
		/// </summary>
		internal UINT NumCTMFields { get; }

		/// <summary>
		///     <para>Size (in bytes) of fixed portion of NStruct.</para>
		///     <remarks>
		///         <para>
		///             Equal to <see cref="Marshal.SizeOf(Type)" /> and (<see cref="EEClass" />)
		///             <see cref="EEClass.NativeSize" />
		///         </para>
		///     </remarks>
		/// </summary>
		internal uint NativeSize => m_cbNativeSize;

		/// <summary>
		///     <remarks>
		///         <para>Equal to <see cref="Unsafe.SizeOf{T}" /> </para>
		///     </remarks>
		/// </summary>
		internal uint ManagedSize => m_cbManagedSize;

		internal LayoutFlags Flags       => (LayoutFlags) m_bFlags;
		internal bool        ZeroSized   => Flags.HasFlag(LayoutFlags.ZeroSized);
		internal bool        IsBlittable => Flags.HasFlag(LayoutFlags.Blittable);

		public override string ToString()
		{
			var table = new ConsoleTable("Field", "Value");

			table.AddRow("Native size", m_cbNativeSize);
			table.AddRow("Managed size", m_cbManagedSize);
			table.AddRow("Flags", EnumUtil.CreateFlagsString(m_bFlags, Flags));
			table.AddRow("Packing size", m_cbPackingSize);

			return table.ToMarkDownString();
		}
	}
}