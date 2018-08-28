#define EXTRA_FIELDS

#region

using System;
using System.Runtime.InteropServices;
using RazorCommon;
using RazorCommon.Strings;
using RazorSharp.Pointers;
using RazorSharp.Utilities.Exceptions;

#endregion

// ReSharper disable InconsistentNaming
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable BuiltInTypeReferenceStyle

namespace RazorSharp.CLR.Structures
{

	#region

	using DWORD = UInt32;
	using WORD = UInt16;

	#endregion


	/// <summary>
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
	///         Do not dereference. This should be kept a pointer and should not be copied.
	///     </remarks>
	/// </summary>
	[StructLayout(LayoutKind.Explicit)]
	public unsafe struct MethodTable
	{

		#region Properties and Accessors

		#region Flags

		private DWORD FlagsValue {
			get {
				IntPtr dwPtr = Unsafe.AddressOf(ref m_dwFlags);
				return *(DWORD*) dwPtr;
			}
		}

		private WORD FlagsLowValue => m_dwFlags.Flags;
		private WORD Flags2Value   => m_wFlags2;

		public MethodTableFlags Flags => (MethodTableFlags) FlagsValue;


		/// <summary>
		///     Note: these may not be accurate
		/// </summary>
		public MethodTableFlagsLow FlagsLow => (MethodTableFlagsLow) FlagsLowValue;


		public MethodTableFlags2 Flags2 => (MethodTableFlags2) Flags2Value;

		#endregion

		/// <summary>
		///     <para>The size of an individual element when this type is an array or string.</para>
		///     <example>
		///         If this type is a <c>string</c>, the component size will be 2. (<c>sizeof(char)</c>)
		///     </example>
		///     <returns>
		///         <c>-1</c> if <c>!</c><see cref="HasComponentSize" />, component size otherwise
		///     </returns>
		/// </summary>
		public short ComponentSize => HasComponentSize ? (short) m_dwFlags.ComponentSize : (short) -1;

		/// <summary>
		///     The base size of this class when allocated on the heap. Note that for value types
		///     <see cref="BaseSize" /> returns the size of instance fields for a boxed value, and
		///     <see cref="NumInstanceFieldBytes" /> for an unboxed value.
		/// </summary>
		public DWORD BaseSize => m_BaseSize;

		/// <summary>
		///     Class token if it fits into 16-bits. If this is (WORD)-1, the class token is stored in the TokenOverflow optional
		///     member.
		/// </summary>
		public WORD Token => m_wToken;

		/// <summary>
		///     The number of virtual methods in this type (<c>4</c> by default; from <see cref="Object" />)
		/// </summary>
		public WORD NumVirtuals => m_wNumVirtuals;

		/// <summary>
		///     The number of interfaces this type implements
		/// </summary>
		public WORD NumInterfaces => m_wNumInterfaces;

		/// <summary>
		///     The parent type's <see cref="MethodTable" />.
		/// </summary>
		/// <exception cref="NotImplementedException">If the type is an indirect parent</exception>
		public Pointer<MethodTable> Parent {
			// On Linux ARM is a RelativeFixupPointer. Otherwise,
			// Parent PTR_MethodTable if enum_flag_HasIndirectParent is not set. Pointer to indirection cell
			// if enum_flag_HasIndirectParent is set. The indirection is offset by offsetof(MethodTable, m_pParentMethodTable).
			// It allows casting helpers to go through parent chain naturally. Casting helper do not need need the explicit check
			// for enum_flag_HasIndirectParentMethodTable.
			get {
				if (!Flags.HasFlag(MethodTableFlags.HasIndirectParent)) {
					return m_pParentMethodTable;
				}

				throw new NotImplementedException("Parent is indirect");
			}
		}

		// todo
		internal void* Module => m_pLoaderModule;

		/// <summary>
		///     <para>The corresponding <see cref="EEClass" /> to this <see cref="MethodTable" />.</para>
		///     <remarks>
		///         <para>
		///             Source: /src/vm/methodtable.inl: 22
		///         </para>
		///     </remarks>
		/// </summary>
		/// <exception cref="NotImplementedException">
		///     If the union type is not <see cref="LowBits.EEClass" /> or
		///     <see cref="LowBits.MethodTable" />
		/// </exception>
		public Pointer<EEClass> EEClass {
			get {
				switch (UnionType) {
					case LowBits.EEClass:
						return m_pEEClass;
					case LowBits.MethodTable:
						return Canon.Reference.EEClass;
					default:
						throw new NotImplementedException("EEClass union type is not implemented");
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
		public Pointer<MethodTable> Canon {
			get {
				switch (UnionType) {
					case LowBits.MethodTable:
						return (MethodTable*) PointerUtils.Subtract(m_pCanonMT, 2);
					case LowBits.EEClass:
					{
						fixed (MethodTable* mt = &this) {
							return mt;
						}
					}
					default:
						throw new NotImplementedException("Canon MT could not be accessed");
				}
			}
		}

		/// <summary>
		///     Element type handle of an individual element if this is the <see cref="MethodTable" /> of an array.
		/// </summary>
		/// <exception cref="RuntimeException">If this is not an array <see cref="MethodTable" />.</exception>
		public Pointer<MethodTable> ElementTypeHandle {
			get {
				if (IsArray) {
					return (MethodTable*) m_ElementTypeHnd;
				}

				throw new RuntimeException("Element type handles cannot be accessed when type is not an array");
			}
		}

		public bool HasComponentSize => Flags.HasFlag(MethodTableFlags.HasComponentSize);
		public bool IsArray          => Flags.HasFlag(MethodTableFlags.Array);
		public bool IsStringOrArray  => HasComponentSize;
		public bool IsBlittable      => EEClass.Reference.IsBlittable;
		public bool IsString         => HasComponentSize && !IsArray;

		/// <summary>
		///     <remarks>
		///         Address-sensitive
		///     </remarks>
		/// </summary>
		public Type RuntimeType => CLRFunctions.JIT_GetRuntimeType(Unsafe.AddressOf(ref this).ToPointer());

		/// <summary>
		///     The number of instance fields in this type.
		/// </summary>
		public int NumInstanceFields => EEClass.Reference.NumInstanceFields;

		/// <summary>
		///     The number of <c>static</c> fields in this type.
		/// </summary>
		public int NumStaticFields => EEClass.Reference.NumStaticFields;

		public int NumNonVirtualSlots => EEClass.Reference.NumNonVirtualSlots;


		/// <summary>
		///     Number of methods in this type.
		/// </summary>
		public int NumMethods => EEClass.Reference.NumMethods;


		/// <summary>
		///     The size of the instance fields in this type. This is the unboxed size of the type if the object is boxed.
		///     (Minus padding and overhead of the base size.)
		/// </summary>
		public int NumInstanceFieldBytes => (int) BaseSize - EEClass.Reference.BaseSizePadding;

		/// <summary>
		///     Array of <see cref="FieldDesc" />s for this type.
		/// </summary>
		internal FieldDesc* FieldDescList => EEClass.Reference.FieldDescList;

		/// <summary>
		///     Length of the <see cref="FieldDescList" />
		/// </summary>
		public int FieldDescListLength => EEClass.Reference.FieldDescListLength;

		// todo
		internal MethodDescChunk* MethodDescChunkList => EEClass.Reference.MethodDescChunkList;

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

		private LowBits UnionType {
			get {
				long l = (long) m_pEEClass;
				return (LowBits) (l & UnionMask);
			}
		}

		#endregion

		public override string ToString()
		{
			//var lowFlags = String.Join(", ", TableFlagsLow.GetFlags().Distinct());

			ConsoleTable table = new ConsoleTable("Field", "Value");

			table.AddRow("Name", RuntimeType.Name);

			table.AddRow("Base size", m_BaseSize);
			if (HasComponentSize) {
				table.AddRow("Component size", m_dwFlags.ComponentSize);
			}

			table.AddRow("Flags", $"{FlagsValue} ({Flags.Join()})");
			table.AddRow("Flags 2", $"{Flags2Value} ({Flags2.Join()})");
			table.AddRow("Low flags", $"{FlagsLowValue} ({FlagsLow})");
			table.AddRow("Token", m_wToken);

			if (m_pParentMethodTable != null) {
				table.AddRow("Parent MT", Hex.ToHex(m_pParentMethodTable));
			}

			table.AddRow("Module", Hex.ToHex(m_pLoaderModule));

			//table.AddRow("m_pWriteableData", Hex.ToHex(m_pWriteableData));

			table.AddRow("Union type", UnionType);
			table.AddRow("EEClass", Hex.ToHex(EEClass.Address));
			table.AddRow("Canon MT", Hex.ToHex(Canon.Address));

			if (IsArray) {
				table.AddRow("Element type handle", Hex.ToHex(m_ElementTypeHnd));
			}


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
			table.AddRow("Blittable", EEClass.Reference.IsBlittable ? StringUtils.Check : StringUtils.BallotX);


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
			if (ReferenceEquals(null, obj)) {
				return false;
			}

			return obj is MethodTable && Equals((MethodTable) obj);
		}

		#endregion


	}

}