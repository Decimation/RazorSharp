using System;
using System.Runtime.InteropServices;
using RazorCommon.Diagnostics;
using RazorSharp.CoreClr.Enums;
using RazorSharp.CoreClr.Enums.MethodTable;
using RazorSharp.CoreClr.Structures.EE;
using RazorSharp.Memory.Calling.Symbols;
using RazorSharp.Memory.Calling.Symbols.Attributes;
using RazorSharp.Pointers;

// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable BuiltInTypeReferenceStyle
// ReSharper disable FieldCanBeMadeReadOnly.Local

namespace RazorSharp.CoreClr.Structures
{
	#region

	using DWORD = UInt32;
	using WORD = UInt16;

	#endregion

	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct MethodTable32
	{
		static MethodTable32()
		{
			Symcall.BindQuick(typeof(MethodTable32));
		}
		public bool Equals(MethodTable32 other)
		{
			return m_dwFlags.Equals(other.m_dwFlags) && m_BaseSize == other.m_BaseSize &&
			       m_wFlags2 == other.m_wFlags2 && m_wToken == other.m_wToken &&
			       m_wNumVirtuals == other.m_wNumVirtuals && m_wNumInterfaces == other.m_wNumInterfaces &&
			       m_pParentMethodTable == other.m_pParentMethodTable && m_pLoaderModule == other.m_pLoaderModule &&
			       m_pWriteableData == other.m_pWriteableData && m_pEEClass == other.m_pEEClass &&
			       m_pPerInstInfo == other.m_pPerInstInfo && m_pInterfaceMap == other.m_pInterfaceMap;
		}
		
		

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is MethodTable32 other && Equals(other);
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
				hashCode = (hashCode * 397) ^ unchecked((int) (long) m_pPerInstInfo);
				hashCode = (hashCode * 397) ^ unchecked((int) (long) m_pInterfaceMap);
				return hashCode;
			}
		}

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

		internal bool IsParentIndirect => Flags.HasFlag(MethodTableFlags.HasIndirectParent);

		[ClrSymcall(Symbol = "MethodTable::GetParentMethodTable", FullyQualified = true)]
		private MethodTable32* GetParentMethodTable()
		{
			return null;
		}

		internal Pointer<MethodTable32> Parent {
			get {
				if (!IsParentIndirect)
					return m_pParentMethodTable;
				else {
					return GetParentMethodTable();
				}
			}
		}


		// todo
		internal Pointer<byte> Module => m_pLoaderModule;

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


		internal Pointer<MethodTable32> Canon {
			get {
				switch (UnionType) {
					case LowBits.MethodTable:
						Pointer<MethodTable32> pCanon = m_pCanonMT;
						pCanon.Subtract(Offsets.CANON_MT_UNION_MT_OFFSET);
						return pCanon;
					case LowBits.EEClass:
					{
						fixed (MethodTable32* mt = &this) {
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


		internal Pointer<MethodTable32> ElementTypeHandle {
			get {
				Conditions.Requires(IsArray);
				return (MethodTable32*) m_ElementTypeHnd;
			}
		}

		internal bool   HasComponentSize => Flags.HasFlag(MethodTableFlags.HasComponentSize);
		internal bool   IsArray          => Flags.HasFlag(MethodTableFlags.Array);
		internal bool   IsStringOrArray  => HasComponentSize;
		internal bool   IsBlittable      => EEClass.Reference.IsBlittable;
		internal bool   IsString         => HasComponentSize && !IsArray;
		internal bool   ContainsPointers => Flags.HasFlag(MethodTableFlags.ContainsPointers);
		internal string Name             => RuntimeType.Name;

		// internal name: GetTypeDefRid
		internal int Token => Constants.TokenFromRid(OrigToken, CorTokenType.TypeDef);


		internal Type RuntimeType {
			get {
				fixed (MethodTable32* value = &this) {
					return ClrFunctions.JIT_GetRuntimeType_Safe((MethodTable*) value);
				}
			}
		}


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

		public DWFlags        m_dwFlags;
		public DWORD          m_BaseSize;
		public WORD           m_wFlags2;
		public WORD           m_wToken;
		public WORD           m_wNumVirtuals;
		public WORD           m_wNumInterfaces;
		public MethodTable32* m_pParentMethodTable;
		public void*          m_pLoaderModule;
		public void*          m_pWriteableData;


		/// <summary>
		/// Union
		/// 
		/// EEClass* 		m_pEEClass
		/// MethodTable32* 	m_pCanonMT
		/// </summary>
		public EEClass* m_pEEClass;

		public MethodTable32* m_pCanonMT => (MethodTable32*) m_pEEClass;

		/// <summary>
		/// Union
		/// 
		/// void* 		m_pPerInstInfo
		/// void* 		m_ElementTypeHnd
		/// void*		m_pMultipurposeSlot1
		/// </summary>
		public void* m_pPerInstInfo;

		public void* m_ElementTypeHnd     => m_pPerInstInfo;
		public void* m_pMultipurposeSlot1 => m_pPerInstInfo;

		/// <summary>
		/// Union
		/// 
		/// void* 		m_pInterfaceMap
		/// void* 		m_pMultipurposeSlot2
		/// </summary>
		public void* m_pInterfaceMap;

		public void* m_pMultipurposeSlot2 => m_pInterfaceMap;
	}
}