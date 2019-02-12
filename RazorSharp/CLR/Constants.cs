#region

using System;
using RazorSharp.CLR.Structures;
using RazorSharp.CLR.Structures.EE;

#endregion

// ReSharper disable UnusedMember.Global
// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming

namespace RazorSharp.CLR
{
	internal static unsafe class Constants
	{
		/// <summary>
		///     <para>Minimum GC object heap size</para>
		///     <para>Sources:</para>
		///     <list type="bullet">
		///         <item>
		///             <description>/src/vm/object.h: 119</description>
		///         </item>
		///     </list>
		/// </summary>
		internal static readonly int MinObjectSize = 2 * IntPtr.Size + sizeof(ObjHeader);

		//todo
//		private const uint GC_MARKED = 0x1;

		/// <summary>
		///     <para>Sources:</para>
		///     <list type="bullet">
		///         <item>
		///             <description>src/vm/siginfo.cpp: 63</description>
		///         </item>
		///     </list>
		///     <exception cref="Exception">If size is unknown</exception>
		/// </summary>
		internal static int SizeOfCorElementType(CorElementType t)
		{
			const int specialSize = -1;

			switch (t) {
				case CorElementType.Void:
					return 0;

				case CorElementType.Boolean:
					return sizeof(bool);

				case CorElementType.Char:
					return sizeof(char);

				case CorElementType.I1:
					return sizeof(sbyte);
				case CorElementType.U1:
					return sizeof(byte);

				case CorElementType.I2:
					return sizeof(short);
				case CorElementType.U2:
					return sizeof(ushort);

				case CorElementType.I4:
					return sizeof(int);
				case CorElementType.U4:
					return sizeof(uint);

				case CorElementType.I8:
					return sizeof(long);
				case CorElementType.U8:
					return sizeof(ulong);

				case CorElementType.R4:
					return sizeof(float);
				case CorElementType.R8:
					return sizeof(double);

				case CorElementType.String:
				case CorElementType.Ptr:
				case CorElementType.ByRef:
				case CorElementType.Class:
				case CorElementType.Array:
				case CorElementType.I:
				case CorElementType.U:
				case CorElementType.FnPtr:
				case CorElementType.Object:
				case CorElementType.SzArray:
					return IntPtr.Size;


				case CorElementType.End:
				case CorElementType.ValueType:
				case CorElementType.Var:
				case CorElementType.GenericInst:
				case CorElementType.CModReqd:
				case CorElementType.CModOpt:
				case CorElementType.Internal:
				case CorElementType.MVar:
					return specialSize;

				case CorElementType.TypedByRef:
					return IntPtr.Size * 2;

				case CorElementType.Max:
				case CorElementType.Modifier:
				case CorElementType.Sentinel:
				case CorElementType.Pinned:
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(t), t, null);
			}

			throw new Exception($"Size for CorElementType {t} is unknown");
		}

		internal static bool IsPrimitive(CorElementType cet)
		{
			return cet >= CorElementType.Boolean && cet <= CorElementType.R8
			       || cet == CorElementType.I || cet == CorElementType.U
			       || cet == CorElementType.Ptr || cet == CorElementType.FnPtr;
		}

		private static CorElementType TypeToCorType(Type t)
		{
			switch (Type.GetTypeCode(t)) {
				case TypeCode.Empty:
				case TypeCode.DBNull:
					goto case default;
				case TypeCode.Boolean:
					return CorElementType.Boolean;
				case TypeCode.Char:
					return CorElementType.Char;
				case TypeCode.SByte:
					return CorElementType.I1;
				case TypeCode.Byte:
					return CorElementType.U1;
				case TypeCode.Int16:
					return CorElementType.I2;
				case TypeCode.UInt16:
					return CorElementType.U2;
				case TypeCode.Int32:
					return CorElementType.I4;
				case TypeCode.UInt32:
					return CorElementType.U4;
				case TypeCode.Int64:
					return CorElementType.I8;
				case TypeCode.UInt64:
					return CorElementType.U8;
				case TypeCode.Single:
					return CorElementType.R4;
				case TypeCode.Double:
					return CorElementType.R8;
				case TypeCode.DateTime:
				case TypeCode.Decimal:
					return CorElementType.ValueType;
				case TypeCode.Object:
				case TypeCode.String:
					return CorElementType.Class;
				default:
					throw new ArgumentOutOfRangeException(
						$"{t.Name} has not been mapped to {nameof(CorElementType)}.");
			}
		}

		internal static CorElementType TypeToCorType<T>()
		{
			return TypeToCorType(typeof(T));
		}

		internal static int RidFromToken(int tk)
		{
			// #define RidFromToken(tk) ((RID) ((tk) & 0x00ffffff))
			return tk & 0x00ffffff;
		}

		internal static long TypeFromToken(int tk)
		{
			// #define TypeFromToken(tk) ((ULONG32)((tk) & 0xff000000))
			return tk & 0xff000000;
		}

		internal static int TokenFromRid(int rid, CorTokenType tktype)
		{
			return rid | (int) tktype;
		}
	}

	#region FieldDesc

	/// <summary>
	///     <remarks>
	///         Use with <see cref="FieldDesc.ProtectionInt" />
	///     </remarks>
	/// </summary>
	public enum ProtectionLevel
	{
		Private           = 4,
		PrivateProtected  = 8,
		Internal          = 12,
		Protected         = 16,
		ProtectedInternal = 20,
		Public            = 24
	}

	#endregion


	/// <summary>
	///     <para>Sources:</para>
	///     <list type="bullet">
	///         <item>
	///             <description>/src/inc/corhdr.h: 1476</description>
	///         </item>
	///     </list>
	/// </summary>
	internal enum CorTokenType
	{
		mdtModule                 = 0x00000000, //
		mdtTypeRef                = 0x01000000, //
		mdtTypeDef                = 0x02000000, //
		mdtFieldDef               = 0x04000000, //
		mdtMethodDef              = 0x06000000, //
		mdtParamDef               = 0x08000000, //
		mdtInterfaceImpl          = 0x09000000, //
		mdtMemberRef              = 0x0a000000, //
		mdtCustomAttribute        = 0x0c000000, //
		mdtPermission             = 0x0e000000, //
		mdtSignature              = 0x11000000, //
		mdtEvent                  = 0x14000000, //
		mdtProperty               = 0x17000000, //
		mdtMethodImpl             = 0x19000000, //
		mdtModuleRef              = 0x1a000000, //
		mdtTypeSpec               = 0x1b000000, //
		mdtAssembly               = 0x20000000, //
		mdtAssemblyRef            = 0x23000000, //
		mdtFile                   = 0x26000000, //
		mdtExportedType           = 0x27000000, //
		mdtManifestResource       = 0x28000000, //
		mdtGenericParam           = 0x2a000000, //
		mdtMethodSpec             = 0x2b000000, //
		mdtGenericParamConstraint = 0x2c000000,
		mdtString                 = 0x70000000, //
		mdtName                   = 0x71000000, //

		mdtBaseType =
			0x72000000 // Leave this on the high end value. This does not correspond to metadata table
	}


	/// <summary>
	///     <para>Sources:</para>
	///     <list type="bullet">
	///         <item>
	///             <description>
	///                 /src/System.Private.CoreLib/src/System/Reflection/MdImport.cs: 22
	///             </description>
	///         </item>
	///         <item>
	///             <description>
	///                 /src/inc/corhdr.h: 863
	///             </description>
	///         </item>
	///         <item>
	///             <description>
	///                 /src/vm/siginfo.cpp: 63
	///             </description>
	///         </item>
	///     </list>
	///     <remarks>
	///         Use with <see cref="FieldDesc.CorType" />
	///     </remarks>
	/// </summary>
	public enum CorElementType
	{
		End  = 0x00,
		Void = 0x01,

		/// <summary>
		///     bool
		/// </summary>
		Boolean = 0x02,

		/// <summary>
		///     char
		/// </summary>
		Char = 0x03,

		/// <summary>
		///     sbyte
		/// </summary>
		I1 = 0x04,

		/// <summary>
		///     byte
		/// </summary>
		U1 = 0x05,

		/// <summary>
		///     short
		/// </summary>
		I2 = 0x06,

		/// <summary>
		///     ushort
		/// </summary>
		U2 = 0x07,

		/// <summary>
		///     int
		/// </summary>
		I4 = 0x08,

		/// <summary>
		///     uint
		/// </summary>
		U4 = 0x09,

		/// <summary>
		///     long
		/// </summary>
		I8 = 0x0A,

		/// <summary>
		///     ulong
		/// </summary>
		U8 = 0x0B,

		/// <summary>
		///     float
		/// </summary>
		R4 = 0x0C,

		/// <summary>
		///     double
		/// </summary>
		R8 = 0x0D,

		/// <summary>
		///     Note: strings don't actually map to this. They map to <see cref="Class" />
		/// </summary>
		String = 0x0E,

		Ptr   = 0x0F,
		ByRef = 0x10,

		/// <summary>
		///     Struct type
		/// </summary>
		ValueType = 0x11,

		/// <summary>
		///     Reference type (i.e. string, object)
		/// </summary>
		Class = 0x12,

		Var         = 0x13,
		Array       = 0x14,
		GenericInst = 0x15,
		TypedByRef  = 0x16,
		I           = 0x18,
		U           = 0x19,
		FnPtr       = 0x1B,
		Object      = 0x1C,
		SzArray     = 0x1D,
		MVar        = 0x1E,
		CModReqd    = 0x1F,
		CModOpt     = 0x20,
		Internal    = 0x21,
		Max         = 0x22,
		Modifier    = 0x40,
		Sentinel    = 0x41,
		Pinned      = 0x45
	}


	#region MethodTable

	/// <summary>
	///     The value of lowest two bits describe what the union contains
	///     <remarks>
	///         Use with <see cref="Structures.MethodTable.UnionType" />
	///     </remarks>
	/// </summary>
	[Flags]
	public enum LowBits
	{
		/// <summary>
		///     0 - pointer to <see cref="EEClass" />
		///     This <see cref="MethodTable" /> is the canonical method table.
		/// </summary>
		EEClass = 0,

		/// <summary>
		///     1 - not used
		/// </summary>
		Invalid = 1,

		/// <summary>
		///     2 - pointer to canonical <see cref="MethodTable" />.
		/// </summary>
		MethodTable = 2,

		/// <summary>
		///     3 - pointer to indirection cell that points to canonical <see cref="MethodTable" />.
		///     (used only if FEATURE_PREJIT is defined)
		/// </summary>
		Indirection = 3
	}

	/// <summary>
	///     <para>Sources:</para>
	///     <list type="bullet">
	///         <item>
	///             <description>/src/vm/methodtable.h: 3969</description>
	///         </item>
	///     </list>
	///     <remarks>
	///         Use with <see cref="MethodTable.Flags" />
	///     </remarks>
	/// </summary>
	[Flags]
	public enum MethodTableFlags : uint
	{
		Mask             = 0x000F0000,
		Class            = 0x00000000,
		Unused1          = 0x00010000,
		MarshalByRefMask = 0x000E0000,
		MarshalByRef     = 0x00020000,

		/// <summary>
		///     sub-category of MarshalByRef
		/// </summary>
		Contextful = 0x00030000,

		ValueType     = 0x00040000,
		ValueTypeMask = 0x000C0000,

		/// <summary>
		///     sub-category of ValueType
		/// </summary>
		Nullable = 0x00050000,

		/// <summary>
		///     sub-category of ValueType, Enum or primitive value type
		/// </summary>
		PrimitiveValueType = 0x00060000,

		/// <summary>
		///     sub-category of ValueType, Primitive (ELEMENT_TYPE_I, etc.)
		/// </summary>
		TruePrimitive = 0x00070000,

		Array     = 0x00080000,
		ArrayMask = 0x000C0000,

		/// <summary>
		///     sub-category of Array
		/// </summary>
		IfArrayThenSzArray = 0x00020000,

		Interface        = 0x000C0000,
		Unused2          = 0x000D0000,
		TransparentProxy = 0x000E0000,
		AsyncPin         = 0x000F0000,

		/// <summary>
		///     bits that matter for element type mask
		/// </summary>
		ElementTypeMask = 0x000E0000,

		/// <summary>
		///     instances require finalization
		/// </summary>
		HasFinalizer = 0x00100000,

		/// <summary>
		///     Is this type marshalable by the pinvoke marshalling layer
		/// </summary>
		IfNotInterfaceThenMarshalable = 0x00200000,

		/// <summary>
		///     Does the type has optional GuidInfo
		/// </summary>
		IfInterfaceThenHasGuidInfo = 0x00200000,

		/// <summary>
		///     class implements ICastable interface
		/// </summary>
		ICastable = 0x00400000,

		/// <summary>
		///     m_pParentMethodTable has double indirection
		/// </summary>
		HasIndirectParent = 0x00800000,
		ContainsPointers = 0x01000000,

		/// <summary>
		///     can be equivalent to another type
		/// </summary>
		HasTypeEquivalence = 0x02000000,

		/// <summary>
		///     has optional pointer to RCWPerTypeData
		/// </summary>
		HasRCWPerTypeData = 0x04000000,

		/// <summary>
		///     finalizer must be run on Appdomain Unload
		/// </summary>
		HasCriticalFinalizer = 0x08000000,

		Collectible              = 0x10000000,
		ContainsGenericVariables = 0x20000000,

		/// <summary>
		///     class is a com object
		/// </summary>
		ComObject = 0x40000000,

		/// <summary>
		///     This is set if component size is used for flags.
		/// </summary>
		HasComponentSize = 0x80000000,

		/// <summary>
		///     Types that require non-trivial interface cast have this bit set in the category
		/// </summary>
		NonTrivialInterfaceCast = Array
		                          | ComObject
		                          | ICastable
	}

	/// <summary>
	///     <remarks>
	///         Use with <see cref="MethodTable.FlagsLow" />
	///     </remarks>
	/// </summary>
	[Flags]
	public enum MethodTableFlagsLow : ushort
	{
		// We are overloading the low 2 bytes of m_dwFlags to be a component size for Strings
		// and Arrays and some set of flags which we can be assured are of a specified state
		// for Strings / Arrays, currently these will be a bunch of generics flags which don't
		// apply to Strings / Arrays.

		UnusedComponentSize1 = 0x00000001,

		StaticsMask                           = 0x00000006,
		StaticsMask_NonDynamic                = 0x00000000,
		StaticsMask_Dynamic                   = 0x00000002, // dynamic statics (EnC, reflection.emit)
		StaticsMask_Generics                  = 0x00000004, // generics statics
		StaticsMask_CrossModuleGenerics       = 0x00000006, // cross module generics statics (NGen)
		StaticsMask_IfGenericsThenCrossModule = 0x00000002, // helper constant to get rid of unnecessary check

		NotInPZM = 0x00000008, // True if this type is not in its PreferredZapModule

		GenericsMask             = 0x00000030,
		GenericsMask_NonGeneric  = 0x00000000, // no instantiation
		GenericsMask_GenericInst = 0x00000010, // regular instantiation, e.g. List<String>

		GenericsMask_SharedInst =
			0x00000020,                        // shared instantiation, e.g. List<__Canon> or List<MyValueType<__Canon>>
		GenericsMask_TypicalInst = 0x00000030, // the type instantiated at its formal parameters, e.g. List<T>

		HasRemotingVtsInfo = 0x00000080, // Optional data present indicating VTS methods and optional fields

		HasVariance =
			0x00000100, // This is an instantiated type some of whose type parameters are co or contra-variant

		HasDefaultCtor = 0x00000200,

		HasPreciseInitCctors =
			0x00000400, // Do we need to run class constructors at allocation time? (Not perf important, could be moved to EEClass

//#if defined(FEATURE_HFA)
//#if defined(UNIX_AMD64_ABI)
//#error Can't define both FEATURE_HFA and UNIX_AMD64_ABI
//#endif
		IsHFA = 0x00000800, // This type is an HFA (Homogenous Floating-point Aggregate)
//#endif // FEATURE_HFA

//#if defined(UNIX_AMD64_ABI)
//#if defined(FEATURE_HFA)
//#error Can't define both FEATURE_HFA and UNIX_AMD64_ABI
//#endif
		IsRegStructPassed = 0x00000800, // This type is a System V register passed struct.
//#endif // UNIX_AMD64_ABI

		IsByRefLike = 0x00001000,

		// In a perfect world we would fill these flags using other flags that we already have
		// which have a constant value for something which has a component size.
		UnusedComponentSize5 = 0x00002000,
		UnusedComponentSize6 = 0x00004000,
		UnusedComponentSize7 = 0x00008000,

		StringArrayValues = (StaticsMask_NonDynamic & 0xFFFF) |
		                    (NotInPZM & 0) |
		                    (GenericsMask_NonGeneric & 0xFFFF) |
		                    (HasVariance & 0) |
		                    (HasDefaultCtor & 0) |
		                    (HasPreciseInitCctors & 0)
	}

	/// <summary>
	///     <para>Sources:</para>
	///     <list type="bullet">
	///         <item>
	///             <description>/src/vm/methodtable.h: 4049</description>
	///         </item>
	///     </list>
	///     <remarks>
	///         Use with <see cref="MethodTable.Flags2" />
	///     </remarks>
	/// </summary>
	[Flags]
	public enum MethodTableFlags2 : ushort
	{
		MultipurposeSlotsMask    = 0x001F,
		HasPerInstInfo           = 0x0001,
		HasInterfaceMap          = 0x0002,
		HasDispatchMapSlot       = 0x0004,
		HasNonVirtualSlots       = 0x0008,
		HasModuleOverride        = 0x0010,
		IsZapped                 = 0x0020,
		IsPreRestored            = 0x0040,
		HasModuleDependencies    = 0x0080,
		IsIntrinsicType          = 0x0100,
		RequiresDispatchTokenFat = 0x0200,
		HasCctor                 = 0x0400,
		HasCCWTemplate           = 0x0800,

		/// <summary>
		///     Type requires 8-byte alignment (only set on platforms that require this and don't get it implicitly)
		/// </summary>
		RequiresAlign8 = 0x1000,

		HasBoxedRegularStatics                = 0x2000,
		HasSingleNonVirtualSlot               = 0x4000,
		DependsOnEquivalentOrForwardedStructs = 0x8000
	}

	#endregion

	#region ObjHeader

	/// <summary>
	///     <remarks>
	///         Use with <see cref="ObjHeader.SyncBlock" />
	///     </remarks>
	/// </summary>
	[Flags]
	public enum SyncBlockFlags : uint
	{
		BitSblkStringHasNoHighChars = 0x80000000,
		BitSblkAgileInProgress      = 0x80000000,
		BitSblkStringHighCharsKnown = 0x40000000,
		BitSblkStringHasSpecialSort = 0xC0000000,
		BitSblkStringHighCharMask   = 0xC0000000,
		BitSblkFinalizerRun         = 0x40000000,
		BitSblkGcReserve            = 0x20000000,
		BitSblkSpinLock             = 0x10000000,
		BitSblkIsHashOrSyncblkindex = 0x08000000,
		BitSblkIsHashcode           = 0x04000000
	}

	#endregion


	#region EEClass

	internal enum EEClassFieldId : uint
	{
		NumInstanceFields = 0,
		NumMethods,
		NumStaticFields,
		NumHandleStatics,
		NumBoxedStatics,
		NonGCStaticFieldBytes,
		NumThreadStaticFields,
		NumHandleThreadStatics,
		NumBoxedThreadStatics,
		NonGCThreadStaticFieldBytes,
		NumNonVirtualSlots,
		COUNT
	}

	/// <summary>
	///     <para>Sources:</para>
	///     <list type="bullet">
	///         <item>
	///             <description>/src/vm/class.h: 396</description>
	///         </item>
	///     </list>
	///     <remarks>
	///         Use with <see cref="EEClassLayoutInfo.Flags" />
	///     </remarks>
	/// </summary>
	[Flags]
	public enum LayoutFlags : byte
	{
		/// <summary>
		///     TRUE if the GC layout of the class is bit-for-bit identical
		///     to its unmanaged counterpart (i.e. no internal reference fields,
		///     no ansi-unicode char conversions required, etc.) Used to
		///     optimize marshaling.
		/// </summary>
		Blittable = 0x01,

		/// <summary>
		///     Is this type also sequential in managed memory?
		/// </summary>
		ManagedSequential = 0x02,

		/// <summary>
		///     When a sequential/explicit type has no fields, it is conceptually
		///     zero-sized, but actually is 1 byte in length. This holds onto this
		///     fact and allows us to revert the 1 byte of padding when another
		///     explicit type inherits from this type.
		/// </summary>
		ZeroSized = 0x04,

		/// <summary>
		///     The size of the struct is explicitly specified in the meta-data.
		/// </summary>
		HasExplicitSize = 0x08,

		/// <summary>
		///     Whether a native struct is passed in registers.
		/// </summary>
		NativePassInRegisters = 0x10,

		R4HFA = 0x10,
		R8HFA = 0x20
	}

	/// <summary>
	///     <remarks>
	///         Use with <see cref="EEClass.VMFlags" />
	///     </remarks>
	/// </summary>
	[Flags]
	public enum VMFlags : uint
	{
		LayoutDependsOnOtherModules = 0x00000001,
		Delegate                    = 0x00000002,
		FixedAddressVtStatics       = 0x00000020, // Value type Statics in this class will be pinned
		HasLayout                   = 0x00000040,
		IsNested                    = 0x00000080,
		IsEquivalentType            = 0x00000200,

		//   OVERLAYED is used to detect whether Equals can safely optimize to a bit-compare across the structure.
		HasOverlayedFields = 0x00000400,

		// Set this if this class or its parent have instance fields which
		// must be explicitly inited in a constructor (e.g. pointers of any
		// kind, gc or native).
		//
		// Currently this is used by the verifier when verifying value classes
		// - it's ok to use uninitialised value classes if there are no
		// pointer fields in them.
		HasFieldsWhichMustBeInited = 0x00000800,

		UnsafeValueType = 0x00001000,

		BestFitMappingInited =
			0x00002000,                     // VMFLAG_BESTFITMAPPING and VMFLAG_THROWONUNMAPPABLECHAR are valid only if this is set
		BestFitMapping        = 0x00004000, // BestFitMappingAttribute.Value
		ThrowOnUnmappableChar = 0x00008000, // BestFitMappingAttribute.ThrowOnUnmappableChar

		// unused                              = 0x00010000,
		NoGuid             = 0x00020000,
		HasNonPublicFields = 0x00040000,

		// unused                              = 0x00080000,
		ContainsStackPtr = 0x00100000,
		PreferAlign8     = 0x00200000, // Would like to have 8-byte alignment
		// unused                              = 0x00400000,

		SparseForCominterop = 0x00800000,

		// interfaces may have a coclass attribute
		HasCoClassAttrib   = 0x01000000,
		ComEventItfMask    = 0x02000000, // class is a special COM event interface
		ProjectedFromWinRT = 0x04000000,
		ExportedToWinRT    = 0x08000000,

		// This one indicates that the fields of the valuetype are
		// not tightly packed and is used to check whether we can
		// do bit-equality on value types to implement ValueType::Equals.
		// It is not valid for classes, and only matters if ContainsPointer
		// is false.
		NotTightlyPacked = 0x10000000,

		// True if methoddesc on this class have any real (non-interface) methodimpls
		ContainsMethodImpls        = 0x20000000,
		MarshalingTypeMask         = 0xc0000000,
		MarshalingTypeInhibit      = 0x40000000,
		MarshalingTypeFreeThreaded = 0x80000000,
		MarshalingTypeStandard     = 0xc0000000
	}

	#endregion


	#region MethodDesc

	/// <summary>
	///     <para>Sources:</para>
	///     <list type="bullet">
	///         <item>
	///             <description>/src/vm/method.hpp: 1701</description>
	///         </item>
	///     </list>
	///     <remarks>
	///         Use with <see cref="MethodDesc.Flags2" />
	///     </remarks>
	/// </summary>
	[Flags]
	public enum MethodDescFlags2 : byte
	{
		/// <summary>
		///     The method entrypoint is stable (either precode or actual code)
		/// </summary>
		HasStableEntryPoint = 0x01,

		/// <summary>
		///     implies that HasStableEntryPoint is set.
		///     Precode has been allocated for this method
		/// </summary>
		HasPrecode = 0x02,

		IsUnboxingStub = 0x04,

		/// <summary>
		///     Has slot for native code
		/// </summary>
		HasNativeCodeSlot = 0x08,

		/// <summary>
		///     Jit may expand method as an intrinsic
		/// </summary>
		IsJitIntrinsic = 0x10
	}

	/// <summary>
	///     <para>Sources:</para>
	///     <list type="bullet">
	///         <item>
	///             <description>/src/vm/method.hpp: 1686</description>
	///         </item>
	///     </list>
	///     <remarks>
	///         Use with <see cref="MethodDesc.Flags3" />
	///     </remarks>
	/// </summary>
	[Flags]
	public enum MethodDescFlags3 : ushort
	{
		TokenRemainderMask = 0x3FFF,

		// These are separate to allow the flags space available and used to be obvious here
		// and for the logic that splits the token to be algorithmically generated based on the
		// #define

		/// <summary>
		///     Indicates that a type-forwarded type is used as a valuetype parameter (this flag is only valid for ngenned items)
		/// </summary>
		HasForwardedValuetypeParameter = 0x4000,

		/// <summary>
		///     Indicates that all typeref's in the signature of the method have been resolved to typedefs (or that process failed)
		///     (this flag is only valid for non-ngenned methods)
		/// </summary>
		ValueTypeParametersWalked = 0x4000,

		/// <summary>
		///     Indicates that we have verified that there are no equivalent valuetype parameters for this method
		/// </summary>
		DoesNotHaveEquivalentValuetypeParameters = 0x8000
	}

	/// <summary>
	///     <remarks>
	///         Use with <see cref="MethodDesc.Classification" />
	///     </remarks>
	/// </summary>
	public enum MethodClassification
	{
		/// <summary>
		///     IL
		/// </summary>
		IL = 0,

		/// <summary>
		///     FCall(also includes tlbimped ctor, Delegate ctor)
		/// </summary>
		FCall = 1,


		/// <summary>
		///     N/Direct
		/// </summary>
		NDirect = 2,


		/// <summary>
		///     Special method; implementation provided by EE (like Delegate Invoke)
		/// </summary>
		EEImpl = 3,

		/// <summary>
		///     Array ECall
		/// </summary>
		Array = 4,

		/// <summary>
		///     Instantiated generic methods, including descriptors
		///     for both shared and unshared code (see InstantiatedMethodDesc)
		/// </summary>
		Instantiated = 5,


//#ifdef FEATURE_COMINTEROP
		// This needs a little explanation.  There are MethodDescs on MethodTables
		// which are Interfaces.  These have the mdcInterface bit set.  Then there
		// are MethodDescs on MethodTables that are Classes, where the method is
		// exposed through an interface.  These do not have the mdcInterface bit set.
		//
		// So, today, a dispatch through an 'mdcInterface' MethodDesc is either an
		// error (someone forgot to look up the method in a class' VTable) or it is
		// a case of COM Interop.

		ComInterop = 6,

//#endif                 // FEATURE_COMINTEROP

		/// <summary>
		///     For <see cref="MethodDesc" /> with no metadata behind
		/// </summary>
		Dynamic = 7,
		Count
	}

	/// <summary>
	///     <remarks>
	///         Use with <see cref="MethodDesc.Flags" />
	///     </remarks>
	/// </summary>
	[Flags]
	public enum MethodDescClassification : ushort
	{
		/// <summary>
		///     Method is <see cref="MethodClassification.IL" />, <see cref="MethodClassification.FCall" /> etc., see
		///     <see cref="MethodClassification" /> above.
		/// </summary>
		Classification = 0x0007,
		ClassificationCount = Classification + 1,

		// Note that layout of code:MethodDesc::s_ClassificationSizeTable depends on the exact values
		// of mdcHasNonVtableSlot and mdcMethodImpl

		/// <summary>
		///     Has local slot (vs. has real slot in MethodTable)
		/// </summary>
		HasNonVtableSlot = 0x0008,

		/// <summary>
		///     Method is a body for a method impl (MI_MethodDesc, MI_NDirectMethodDesc, etc)
		///     where the function explicitly implements IInterface.foo() instead of foo().
		/// </summary>
		MethodImpl = 0x0010,

		/// <summary>
		///     Method is static
		/// </summary>
		Static = 0x0020,

		// unused                           = 0x0040,
		// unused                           = 0x0080,
		// unused                           = 0x0100,
		// unused                           = 0x0200,

		// Duplicate method. When a method needs to be placed in multiple slots in the
		// method table, because it could not be packed into one slot. For eg, a method
		// providing implementation for two interfaces, MethodImpl, etc
		Duplicate = 0x0400,

		/// <summary>
		///     Has this method been verified?
		/// </summary>
		VerifiedState = 0x0800,

		/// <summary>
		///     Is the method verifiable? It needs to be verified first to determine this
		/// </summary>
		Verifiable = 0x1000,

		/// <summary>
		///     Is this method ineligible for inlining?
		/// </summary>
		NotInline = 0x2000,

		/// <summary>
		///     Is the method synchronized
		/// </summary>
		Synchronized = 0x4000,

		/// <summary>
		///     Does the method's slot number require all 16 bits
		/// </summary>
		RequiresFullSlotNumber = 0x8000
	}


	internal enum MbMask
	{
		PackedMbLayoutMbMask       = 0x01FFFF,
		PackedMbLayoutNameHashMask = 0xFE0000
	}

	#endregion

	// todo: compare to CorTokenType
	public enum TokenType : uint
	{
		Module                 = 0x00000000,
		TypeRef                = 0x01000000,
		TypeDef                = 0x02000000,
		Field                  = 0x04000000,
		Method                 = 0x06000000,
		Param                  = 0x08000000,
		InterfaceImpl          = 0x09000000,
		MemberRef              = 0x0a000000,
		CustomAttribute        = 0x0c000000,
		Permission             = 0x0e000000,
		Signature              = 0x11000000,
		Event                  = 0x14000000,
		Property               = 0x17000000,
		ModuleRef              = 0x1a000000,
		TypeSpec               = 0x1b000000,
		Assembly               = 0x20000000,
		AssemblyRef            = 0x23000000,
		File                   = 0x26000000,
		ExportedType           = 0x27000000,
		ManifestResource       = 0x28000000,
		GenericParam           = 0x2a000000,
		MethodSpec             = 0x2b000000,
		GenericParamConstraint = 0x2c000000,

		Document               = 0x30000000,
		MethodDebugInformation = 0x31000000,
		LocalScope             = 0x32000000,
		LocalVariable          = 0x33000000,
		LocalConstant          = 0x34000000,
		ImportScope            = 0x35000000,
		StateMachineMethod     = 0x36000000,
		CustomDebugInformation = 0x37000000,

		String = 0x70000000
	}
}