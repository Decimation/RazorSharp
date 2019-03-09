#define EXTRA_FIELDS

#region

using System;
using System.Runtime.InteropServices;
using RazorCommon;
using RazorCommon.Utilities;
using RazorSharp.Clr.Meta;
using RazorSharp.Clr.Structures.EE;
using RazorSharp.Pointers;
using RazorSharp.Utilities.Exceptions;
using static RazorSharp.Clr.Offsets;

#endregion

// ReSharper disable InconsistentNaming
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable BuiltInTypeReferenceStyle

namespace RazorSharp.Clr.Structures
{
	#region

	using DWORD = UInt32;
	using WORD = UInt16;

	#endregion


	/// <summary>
	///     <para>
	///         CLR <see cref="MethodTable" />. Functionality is implemented in this <c>struct</c> and exposed via
	///         <see cref="MetaType" />
	///     </para>
	///     <para>Internal representation: <see cref="RuntimeTypeHandle.Value" /></para>
	///     <para>Corresponding files:</para>
	///     <list type="bullet">
	///         <item>
	///             <description>/src/vm/methodtable.h</description>
	///         </item>
	///         <item>
	///             <description>/src/vm/methodtable.cpp</description>
	///         </item>
	///         <item>
	///             <description>/src/vm/methodtable.inl</description>
	///         </item>
	///         <item>
	///             <description>/src/gc/env/gcenv.object.h</description>
	///         </item>
	///     </list>
	///     <para>Lines of interest:</para>
	///     <list type="bullet">
	///         <item>
	///             <description>/src/vm/methodtable.h: 4166</description>
	///         </item>
	///     </list>
	///     <remarks>
	///         This should only be accessed via <see cref="Pointer{T}" />
	///     </remarks>
	/// </summary>
	[StructLayout(LayoutKind.Explicit)]
	internal unsafe struct MethodTable
	{
		#region Properties and Accessors

		#region Flags

		private DWORD FlagsValue {
			get {
				var dwPtr = Unsafe.AddressOf(ref m_dwFlags).Address;
				return *(DWORD*) dwPtr;
			}
		}

		private WORD FlagsLowValue => m_dwFlags.Flags;
		private WORD Flags2Value   => m_wFlags2;

		internal MethodTableFlags Flags => (MethodTableFlags) FlagsValue;


		/// <summary>
		///     Note: these may not be accurate
		/// </summary>
		internal MethodTableFlagsLow FlagsLow => (MethodTableFlagsLow) FlagsLowValue;


		internal MethodTableFlags2 Flags2 => (MethodTableFlags2) Flags2Value;

		#endregion


		internal short ComponentSize => HasComponentSize ? (short) m_dwFlags.ComponentSize : (short) 0;


		internal int BaseSize => (int) m_BaseSize;

		/// <summary>
		///     Class token if it fits into 16-bits. If this is (WORD)-1, the class token is stored in the TokenOverflow optional
		///     member.
		/// </summary>
		private int OrigToken => m_wToken;


		internal int NumVirtuals => m_wNumVirtuals;


		internal int NumInterfaces => m_wNumInterfaces;

		/// <summary>
		///     The parent type's <see cref="MethodTable" />.
		/// </summary>
		/// <exception cref="NotImplementedException">If the type is an indirect parent</exception>
		internal Pointer<MethodTable> Parent {
			// On Linux ARM is a RelativeFixupPointer. Otherwise,
			// Parent PTR_MethodTable if enum_flag_HasIndirectParent is not set. Pointer to indirection cell
			// if enum_flag_HasIndirectParent is set. The indirection is offset by offsetof(MethodTable, m_pParentMethodTable).
			// It allows casting helpers to go through parent chain naturally. Casting helper do not need need the explicit check
			// for enum_flag_HasIndirectParentMethodTable.
			get {
				if (!Flags.HasFlag(MethodTableFlags.HasIndirectParent)) return m_pParentMethodTable;

				throw new NotImplementedException("Parent is indirect");
			}
		}

		// todo
		internal Pointer<byte> Module => m_pLoaderModule;


		/// <summary>
		///     <para>The corresponding <see cref="EEClass" /> to this <see cref="MethodTable" />.</para>
		///     <remarks>
		///         <para>
		///             Source: /src/vm/methodtable.inl: 22
		///         </para>
		///     </remarks>
		/// </summary>
		/// <exception cref="NotImplementedException">
		///     If the union type is not <see cref="LowBits.EEClass" /> or <see cref="LowBits.MethodTable" />
		/// </exception>
		internal Pointer<EEClass> EEClass {
			get {
				switch (UnionType) {
					case LowBits.EEClass:
						return m_pEEClass;
					case LowBits.MethodTable:
						return Canon.Reference.EEClass;
					case LowBits.Invalid:
					case LowBits.Indirection:
					default:
						throw new NotImplementedException($"Union type {UnionType} is not implemented");
				}
			}
		}

		/// <summary>
		///     <para>The canonical <see cref="MethodTable" />.</para>
		///     <remarks>
		///         <para>Address-sensitive</para>
		///         <para>
		///             Source: /src/vm/methodtable.inl: 1145
		///         </para>
		///     </remarks>
		///     <exception cref="NotImplementedException">
		///         If <see cref="get_UnionType" /> is not <see cref="LowBits.MethodTable" /> or
		///         <see cref="LowBits.EEClass" />
		///     </exception>
		/// </summary>
		internal Pointer<MethodTable> Canon {
			get {
				switch (UnionType) {
					case LowBits.MethodTable:
						Pointer<MethodTable> pCanon = m_pCanonMT;
						pCanon.Subtract(CANON_MT_UNION_MT_OFFSET);
						return pCanon;
					case LowBits.EEClass:
					{
						fixed (MethodTable* mt = &this) {
							return mt;
						}
					}
					case LowBits.Invalid:
					case LowBits.Indirection:
					default:
						throw new NotImplementedException("Canon MT could not be accessed");
				}
			}
		}


		/// <summary>
		///     Element type handle of an individual element if this is the <see cref="MethodTable" /> of an array.
		/// </summary>
		/// <exception cref="RuntimeException">If this is not an array <see cref="MethodTable" />.</exception>
		internal Pointer<MethodTable> ElementTypeHandle {
			get {
				if (IsArray) return (MethodTable*) m_ElementTypeHnd;

				throw new RuntimeException("Element type handles cannot be accessed when type is not an array");
			}
		}

		internal bool   HasComponentSize => Flags.HasFlag(MethodTableFlags.HasComponentSize);
		internal bool   IsArray          => Flags.HasFlag(MethodTableFlags.Array);
		internal bool   IsStringOrArray  => HasComponentSize;
		internal bool   IsBlittable      => EEClass.Reference.IsBlittable;
		internal bool   IsString         => HasComponentSize && !IsArray;
		internal bool   ContainsPointers => Flags.HasFlag(MethodTableFlags.ContainsPointers);
		internal string Name             => RuntimeType.Name;


		internal int Token => Constants.TokenFromRid(OrigToken, CorTokenType.mdtTypeDef);

		// internal name: GetTypeDefRid


		/// <summary>
		///     <remarks>
		///         Address-sensitive
		///     </remarks>
		/// </summary>
		internal Type RuntimeType => Runtime.MethodTableToType(Unsafe.AddressOf(ref this));


		internal int NumInstanceFields => EEClass.Reference.NumInstanceFields;


		internal int NumStaticFields => EEClass.Reference.NumStaticFields;

		internal int NumNonVirtualSlots => EEClass.Reference.NumNonVirtualSlots;


		internal int NumMethods => EEClass.Reference.NumMethods;


		internal int NumInstanceFieldBytes => BaseSize - EEClass.Reference.BaseSizePadding;

		/// <summary>
		///     Array of <see cref="FieldDesc" />s for this type.
		/// </summary>
		internal FieldDesc* FieldDescList => EEClass.Reference.FieldDescList;

		/// <summary>
		///     Length of the <see cref="FieldDescList" />
		/// </summary>
		internal int FieldDescListLength => EEClass.Reference.FieldDescListLength;

		// todo
		internal MethodDescChunk* MethodDescChunkList => EEClass.Reference.MethodDescChunkList;

		#endregion

		#region Fields

		[FieldOffset(0)]
		private DWFlags m_dwFlags;

		[FieldOffset(4)]
		private readonly DWORD m_BaseSize;

		[FieldOffset(8)]
		private readonly WORD m_wFlags2;

		[FieldOffset(10)]
		private readonly WORD m_wToken;

		[FieldOffset(12)]
		private readonly WORD m_wNumVirtuals;

		[FieldOffset(14)]
		private readonly WORD m_wNumInterfaces;

		[FieldOffset(16)]
		private readonly MethodTable* m_pParentMethodTable;

		private const int EECLASS_MT_UNION_OFFSET_32 = 28;
		private const int EECLASS_MT_UNION_OFFSET_64 = 40;

		private const int MISC_UNION_OFFSET_32 = 32;
		private const int MISC_UNION_OFFSET_64 = 48;

		private const int MISC2_UNION_OFFSET_32 = 36;
		private const int MISC2_UNION_OFFSET_64 = 56;
		

		[FieldOffset(16+PTR_SIZE)]
		private readonly void* m_pLoaderModule;

		[FieldOffset(16+(PTR_SIZE*2))]
		private readonly void* m_pWriteableData;

		#region Union

		[FieldOffset(EECLASS_MT_UNION_OFFSET_64)]
		private readonly EEClass* m_pEEClass;

		[FieldOffset(EECLASS_MT_UNION_OFFSET_64)]
		private readonly MethodTable* m_pCanonMT;

		#endregion

		#region Union

		[FieldOffset(MISC_UNION_OFFSET_64)]
		private readonly void* m_pPerInstInfo;

		[FieldOffset(MISC_UNION_OFFSET_64)]
		private readonly void* m_ElementTypeHnd;

		[FieldOffset(MISC_UNION_OFFSET_64)]
		private readonly void* m_pMultipurposeSlot1;

		#endregion

		#region Union

		[FieldOffset(MISC2_UNION_OFFSET_64)]
		private readonly void* m_pInterfaceMap;

		[FieldOffset(MISC2_UNION_OFFSET_64)]
		private readonly void* m_pMultipurposeSlot2;

		#endregion

		/// <summary>
		///     Bit mask for <see cref="UnionType" />
		/// </summary>
		private const long UNION_MASK = 3;

		/// <summary>
		///     Describes what the union at offset <c>40</c> (<see cref="m_pEEClass" />, <see cref="m_pCanonMT" />)
		///     contains.
		/// </summary>
		private LowBits UnionType {
			get {
				long l = (long) m_pEEClass;
				return (LowBits) (l & UNION_MASK);
			}
		}

		#endregion

		public override string ToString()
		{
			var table = new ConsoleTable("Field", "Value");

			table.AddRow("Name", RuntimeType.Name);
			table.AddRow("Base size", m_BaseSize);

			if (HasComponentSize) table.AddRow("Component size", m_dwFlags.ComponentSize);

			table.AddRow("Flags", EnumUtil.CreateFlagsString(FlagsValue, Flags));
			table.AddRow("Flags 2", EnumUtil.CreateFlagsString(Flags2Value, Flags2));
			table.AddRow("Low flags", EnumUtil.CreateFlagsString(FlagsLowValue, FlagsLow));
			table.AddRow("Token", Token);

			if (m_pParentMethodTable != null) table.AddRow("Parent MT", Hex.ToHex(m_pParentMethodTable));

			table.AddRow("Module", Hex.ToHex(m_pLoaderModule));
			table.AddRow("Union type", UnionType);
			table.AddRow("EEClass", Hex.ToHex(EEClass.Address));
			table.AddRow("Canon MT", Hex.ToHex(Canon.Address));

			if (IsArray) table.AddRow("Element type handle", Hex.ToHex(m_ElementTypeHnd));


			// EEClass fields
			table.AddRow("FieldDesc List", Hex.ToHex(FieldDescList));
			table.AddRow("FieldDesc List length", FieldDescListLength);
			table.AddRow("MethodDescChunk List", Hex.ToHex(MethodDescChunkList));
			table.AddRow("Number instance fields", NumInstanceFields);
			table.AddRow("Number static fields", NumStaticFields);
			table.AddRow("Number non virtual slots", NumNonVirtualSlots);
			table.AddRow("Number methods", NumMethods);
			table.AddRow("Number instance field bytes", NumInstanceFieldBytes);


			table.AddRow("Number virtuals", m_wNumVirtuals);
			table.AddRow("Number interfaces", m_wNumInterfaces);

			// EEClass field
			table.AddRow("Blittable", EEClass.Reference.IsBlittable.Prettify());


//			table.RemoveFromRows(0, "0x0", (ushort) 0, (uint) 0);
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
				// ReSharper disable once NonReadonlyMemberInGetHashCode
				// m_dwFlags will never change despite not being readonly
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