#define EXTRA_FIELDS

#region

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using RazorCommon;
using RazorCommon.Strings;
using RazorSharp.Pointers;
using RazorSharp.Utilities;

#endregion

// ReSharper disable InconsistentNaming
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable BuiltInTypeReferenceStyle

namespace RazorSharp.Runtime.CLRTypes
{

	#region

	using DWORD = UInt32;
	using WORD = UInt16;
	using unsigned = UInt32;

	#endregion


	//https://github.com/dotnet/coreclr/blob/db55a1decc1d02538e61eac7db80b7daa351d5b6/src/gc/env/gcenv.object.h


	/// <summary>
	/// Source: https://github.com/dotnet/coreclr/blob/61146b5c5851698e113e936d4e4b51b628095f27/src/vm/methodtable.h#L4166
	/// DO NOT DEREFERENCE<para></para>
	/// Internal representation: TypeHandle.Value
	/// </summary>
	[StructLayout(LayoutKind.Explicit)]
	public unsafe struct MethodTable
	{

		#region Properties and Accessors

		#region Flags

		public DWORD Flags {
			get {
				var dwPtr = Unsafe.AddressOf(ref m_dwFlags);
				return *(DWORD*) dwPtr;
			}
		}

		public WORD LowFlags => m_dwFlags.Flags;
		public WORD Flags2   => m_wFlags2;

		public MethodTableFlags TableFlags => (MethodTableFlags) Flags;

		/// <summary>
		/// Note: these may not be accurate
		/// </summary>
		public MethodTableFlagsLow TableFlagsLow => (MethodTableFlagsLow) LowFlags;


		public MethodTableFlags2 TableFlags2 => (MethodTableFlags2) Flags2;

		#endregion

		/// <summary>
		/// The size of an individual element when this type is an array or string. <para></para>
		///
		/// (i.e. This size will be 2 with strings (sizeof(char)).)
		/// </summary>
		public WORD ComponentSize => HasComponentSize ? m_dwFlags.ComponentSize : (ushort) 0;

		/// <summary>
		/// The base size of this class when allocated on the heap. Note that for value types
		/// GetBaseSize returns the size of instance fields for a boxed value, and
		/// GetNumInstanceFieldsBytes for an unboxed value.
		/// </summary>
		public DWORD BaseSize => m_BaseSize;

		/// <summary>
		/// Class token if it fits into 16-bits. If this is (WORD)-1, the class token is stored in the TokenOverflow optional member.
		/// </summary>
		public WORD Token => m_wToken;

		/// <summary>
		/// The number of virtual methods in this type (4 by default; from Object)
		/// </summary>
		public WORD NumVirtuals => m_wNumVirtuals;

		/// <summary>
		/// The number of interfaces this type implements
		/// </summary>
		public WORD NumInterfaces => m_wNumInterfaces;

		/// <summary>
		/// The parent type's MethodTable.
		/// </summary>
		/// <exception cref="NotImplementedException">If the type is an indirect parent</exception>
		public MethodTable* Parent {
			// On Linux ARM is a RelativeFixupPointer. Otherwise,
			// Parent PTR_MethodTable if enum_flag_HasIndirectParent is not set. Pointer to indirection cell
			// if enum_flag_HasIndirectParent is set. The indirection is offset by offsetof(MethodTable, m_pParentMethodTable).
			// It allows casting helpers to go through parent chain naturally. Casting helper do not need need the explicit check
			// for enum_flag_HasIndirectParentMethodTable.
			get {
				if (!TableFlags.HasFlag(MethodTableFlags.HasIndirectParent)) {
					return m_pParentMethodTable;
				}
				else {
					throw new NotImplementedException("Parent is indirect");
				}
			}
		}

		public Module* Module => m_pLoaderModule;

		/// <summary>
		/// The corresponding EEClass to this MethodTable.<para></para>
		/// Source: https://github.com/dotnet/coreclr/blob/61146b5c5851698e113e936d4e4b51b628095f27/src/vm/methodtable.inl#L22 <para></para>
		///
		/// </summary>
		/// <exception cref="NotImplementedException">If the union type is not EEClass or MethodTable</exception>
		public EEClass* EEClass {
			get {
				switch (UnionType) {
					case Constants.LowBits.EEClass:
						return m_pEEClass;
					case Constants.LowBits.MethodTable:
						return Canon->EEClass;
					default:
						throw new NotImplementedException("EEClass union type is not implemented");
				}
			}
		}

		/// <summary>
		/// The canonical MethodTable.<para></para>
		///
		/// Source: https://github.com/dotnet/coreclr/blob/61146b5c5851698e113e936d4e4b51b628095f27/src/vm/methodtable.inl#L1145 <para></para>
		///
		/// Address-sensitive <para></para>
		/// </summary>
		public MethodTable* Canon {
			get {
				switch (UnionType) {
					case Constants.LowBits.MethodTable:
						return (MethodTable*) PointerUtils.Subtract(m_pCanonMT, 2);
					case Constants.LowBits.EEClass:
					{
						fixed (MethodTable* mt = &this)
							return mt;
					}
					default:
						throw new RuntimeException("Canon MT could not be accessed");
				}
			}
		}

		/// <summary>
		/// Element type handle of an individual element if this is the MethodTable of an array.
		/// </summary>
		/// <exception cref="RuntimeException">If this is not an array MethodTable.</exception>
		public MethodTable* ElementTypeHandle {
			get {
				if (IsArray)
					return (MethodTable*) m_ElementTypeHnd;
				throw new RuntimeException("Element type handles cannot be accessed when type is not an array");
			}
		}

		public bool HasComponentSize => TableFlags.HasFlag(MethodTableFlags.HasComponentSize);
		public bool IsArray          => TableFlags.HasFlag(MethodTableFlags.Array);
		public bool IsStringOrArray  => HasComponentSize;
		public bool IsBlittable      => EEClass->IsBlittable;
		public bool IsString         => HasComponentSize && !IsArray;

		/// <summary>
		/// The number of instance fields in this type.
		/// </summary>
		public int NumInstanceFields => EEClass->NumInstanceFields;

		/// <summary>
		/// The number of static fields in this type.
		/// </summary>
		public int NumStaticFields => EEClass->NumStaticFields;

		public int NumNonVirtualSlots => EEClass->NumNonVirtualSlots;

		/// <summary>
		/// Number of methods in this type.
		/// </summary>
		public int NumMethods => EEClass->NumMethods;

		/// <summary>
		/// The size of the instance fields in this type.
		/// </summary>
		public int NumInstanceFieldBytes => (int) BaseSize - EEClass->BaseSizePadding;

		/// <summary>
		/// Array of FieldDescs for this type.
		/// </summary>
		public FieldDesc* FieldDescList => EEClass->FieldDescList;

		/// <summary>
		/// Length of the FieldDecList
		/// </summary>
		public int FieldDescListLength => EEClass->FieldDescListLength;

		public MethodDescChunk* MethodDescChunkList => EEClass->MethodDescChunkList;

		#endregion

		#region Fields

		[FieldOffset(0)]  private          DWFlags      m_dwFlags;
		[FieldOffset(4)]  private readonly DWORD        m_BaseSize;
		[FieldOffset(8)]  private readonly WORD         m_wFlags2;
		[FieldOffset(10)] private readonly WORD         m_wToken;
		[FieldOffset(12)] private readonly WORD         m_wNumVirtuals;
		[FieldOffset(14)] private readonly WORD         m_wNumInterfaces;
		[FieldOffset(16)] private readonly MethodTable* m_pParentMethodTable;
		[FieldOffset(24)] private readonly Module*      m_pLoaderModule;
		[FieldOffset(32)] private readonly void*        m_pWriteableData;
		[FieldOffset(40)] private readonly EEClass*     m_pEEClass;
		[FieldOffset(40)] private readonly MethodTable* m_pCanonMT;
		[FieldOffset(48)] private readonly void**       m_pPerInstInfo;
		[FieldOffset(48)] private readonly void*        m_ElementTypeHnd;
		[FieldOffset(48)] private readonly void*        m_pMultipurposeSlot1;
		[FieldOffset(56)] private readonly void*        m_pInterfaceMap;
		[FieldOffset(56)] private readonly void*        m_pMultipurposeSlot2;

		private const long UnionMask = 3;

		private Constants.LowBits UnionType {
			get {
				long l = (long) m_pEEClass;
				return (Constants.LowBits) (l & UnionMask);
			}
		}

		#endregion

		public override string ToString()
		{
			const string joinStr = ", ";

			var flags  = String.Join(joinStr, TableFlags.GetFlags());
			var flags2 = String.Join(joinStr, TableFlags2.GetFlags());

			//var lowFlags = String.Join(", ", TableFlagsLow.GetFlags().Distinct());

			var table = new ConsoleTable("Field", "Value");

//			table.AddRow("Name", TypeInfo.Name);
			if (HasComponentSize)
				table.AddRow("Component size", m_dwFlags.ComponentSize);
			table.AddRow("Base size", m_BaseSize);
			table.AddRow("Flags", $"{Flags} ({flags})");
			table.AddRow("Flags 2", $"{Flags2} ({flags2})");
			table.AddRow("Low flags", $"{LowFlags} ({TableFlagsLow})");
			table.AddRow("Token", m_wToken);

			if (m_pParentMethodTable != null)
				table.AddRow("Parent MT", Hex.ToHex(m_pParentMethodTable));

			table.AddRow("Module", Hex.ToHex(m_pLoaderModule));

			//table.AddRow("m_pWriteableData", Hex.ToHex(m_pWriteableData));

			table.AddRow("Union type", UnionType);
			table.AddRow("EEClass", Hex.ToHex(EEClass));
			table.AddRow("Canon MT", Hex.ToHex(Canon));

			if (IsArray)
				table.AddRow("Element type handle", Hex.ToHex(m_ElementTypeHnd));


			//table.AddRow("Multipurpose slot 2", Hex.ToHex(m_pMultipurposeSlot2));


			// EEClass fields
			table.AddRow("FieldDesc List", Hex.ToHex(FieldDescList));
			table.AddRow("FieldDesc List length", FieldDescListLength);
			table.AddRow("MethodDescChunk List", Hex.ToHex(MethodDescChunkList));

			// EEClass fields
			table.AddRow("Number instance fields", NumInstanceFields);
			table.AddRow("Number static fields", NumStaticFields);
			table.AddRow("Number non virtual slots", NumNonVirtualSlots);
			table.AddRow("Number methods", NumMethods);
			table.AddRow("Number instance field bytes", NumInstanceFieldBytes);


			table.AddRow("Number virtuals", m_wNumVirtuals);
			table.AddRow("Number interfaces", m_wNumInterfaces);

			// EEClass field
			table.AddRow("Blittable", EEClass->IsBlittable ? StringUtils.Check : StringUtils.BallotX);


			table.RemoveFromRows(0, "0x0");
			return table.ToMarkDownString();
		}

		#region Equality

		public static bool operator ==(MethodTable a, MethodTable b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(MethodTable a, MethodTable b)
		{
			return !a.Equals(b);
		}

		public override int GetHashCode()
		{
			unchecked {
				int hashCode = m_dwFlags.GetHashCode();
				hashCode = (hashCode * 397) ^ (int) m_BaseSize;
				hashCode = (hashCode * 397) ^ m_wFlags2.GetHashCode();
				hashCode = (hashCode * 397) ^ m_wToken.GetHashCode();
				hashCode = (hashCode * 397) ^ m_wNumVirtuals.GetHashCode();
				hashCode = (hashCode * 397) ^ m_wNumInterfaces.GetHashCode();
				hashCode = (hashCode * 397) ^ unchecked((int) (long) m_pParentMethodTable);
				hashCode = (hashCode * 397) ^ unchecked((int) (long) m_pLoaderModule);
				hashCode = (hashCode * 397) ^ unchecked((int) (long) m_pWriteableData);
				hashCode = (hashCode * 397) ^ unchecked((int) (long) m_pEEClass);
				hashCode = (hashCode * 397) ^ unchecked((int) (long) m_pCanonMT);

				//hashCode = (hashCode * 397) ^ m_slotInfo.GetHashCode();
				//hashCode = (hashCode * 397) ^ m_mapSlot.GetHashCode();
				return hashCode;
			}
		}

		public bool Equals(MethodTable other)
		{
			return m_dwFlags.Equals(other.m_dwFlags)
			       && m_BaseSize == other.m_BaseSize
			       && m_wFlags2 == other.m_wFlags2
			       && m_wToken == other.m_wToken
			       && m_wNumVirtuals == other.m_wNumVirtuals
			       && m_wNumInterfaces == other.m_wNumInterfaces
			       && m_pParentMethodTable == other.m_pParentMethodTable
			       && m_pLoaderModule == other.m_pLoaderModule
			       && m_pWriteableData == other.m_pWriteableData
			       && m_pEEClass == other.m_pEEClass
			       && m_pCanonMT == other.m_pCanonMT;

			// && m_slotInfo.Equals(other.m_slotInfo)
			// && m_mapSlot.Equals(other.m_mapSlot);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is MethodTable && Equals((MethodTable) obj);
		}

		#endregion


	}

}