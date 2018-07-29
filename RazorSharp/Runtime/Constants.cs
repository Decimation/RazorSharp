using System;
using System.Collections.Generic;
using System.Linq;
using RazorSharp.Runtime.CLRTypes;
// ReSharper disable UnusedMember.Global
// ReSharper disable IdentifierTypo

// ReSharper disable InconsistentNaming

namespace RazorSharp.Runtime
{

	public static unsafe class Constants
	{
		internal static MethodTableFlags[] Extract(uint flagBinary)
		{
			var allValues = Enum.GetValues(typeof(MethodTableFlags));
			var list      = new List<MethodTableFlags>();

			foreach (var v in allValues) {
				if ((flagBinary & (uint) v) != 0) {
					list.Add((MethodTableFlags) v);
				}
			}


			return new HashSet<MethodTableFlags>(list).ToArray();
		}

		internal static MethodTableFlags2[] Extract(ushort flagBinary)
		{
			var allValues = Enum.GetValues(typeof(MethodTableFlags2));
			var list      = new List<MethodTableFlags2>();

			foreach (var v in allValues) {
				if ((flagBinary & (ushort) v) != 0) {
					list.Add((MethodTableFlags2) v);
				}
			}


			return new HashSet<MethodTableFlags2>(list).ToArray();
		}

		internal static readonly int MinObjectSize = (2 * IntPtr.Size + sizeof(ObjHeader));

		//todo
		internal const uint GC_MARKED = 0x1;
	}

	/// <summary>
	/// Source: https://github.com/dotnet/coreclr/blob/61146b5c5851698e113e936d4e4b51b628095f27/src/vm/methodtable.h#L3969
	/// Use with: MethodTable::m_dwFlags
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
		/// sub-category of MarshalByRef
		/// </summary>
		Contextful = 0x00030000,

		ValueType     = 0x00040000,
		ValueTypeMask = 0x000C0000,

		/// <summary>
		/// sub-category of ValueType
		/// </summary>
		Nullable = 0x00050000,

		/// <summary>
		/// sub-category of ValueType, Enum or primitive value type
		/// </summary>
		PrimitiveValueType = 0x00060000,

		/// <summary>
		/// sub-category of ValueType, Primitive (ELEMENT_TYPE_I, etc.)
		/// </summary>
		TruePrimitive = 0x00070000,

		Array     = 0x00080000,
		ArrayMask = 0x000C0000,

		/// <summary>
		/// sub-category of Array
		/// </summary>
		IfArrayThenSzArray = 0x00020000,

		Interface        = 0x000C0000,
		Unused2          = 0x000D0000,
		TransparentProxy = 0x000E0000,
		AsyncPin         = 0x000F0000,

		/// <summary>
		/// bits that matter for element type mask
		/// </summary>
		ElementTypeMask = 0x000E0000,

		/// <summary>
		/// instances require finalization
		/// </summary>
		HasFinalizer = 0x00100000,

		/// <summary>
		/// Is this type marshalable by the pinvoke marshalling layer
		/// </summary>
		IfNotInterfaceThenMarshalable = 0x00200000,

		/// <summary>
		/// Does the type has optional GuidInfo
		/// </summary>
		IfInterfaceThenHasGuidInfo = 0x00200000,

		/// <summary>
		/// class implements ICastable interface
		/// </summary>
		ICastable = 0x00400000,

		/// <summary>
		/// m_pParentMethodTable has double indirection
		/// </summary>
		HasIndirectParent = 0x00800000,
		ContainsPointers = 0x01000000,

		/// <summary>
		/// can be equivalent to another type
		/// </summary>
		HasTypeEquivalence = 0x02000000,

		/// <summary>
		/// has optional pointer to RCWPerTypeData
		/// </summary>
		HasRCWPerTypeData = 0x04000000,

		/// <summary>
		/// finalizer must be run on Appdomain Unload
		/// </summary>
		HasCriticalFinalizer = 0x08000000,

		Collectible              = 0x10000000,
		ContainsGenericVariables = 0x20000000,

		/// <summary>
		/// class is a com object
		/// </summary>
		ComObject = 0x40000000,

		/// <summary>
		/// This is set if component size is used for flags.
		/// </summary>
		HasComponentSize = 0x80000000,

		/// <summary>
		/// Types that require non-trivial interface cast have this bit set in the category
		/// </summary>
		NonTrivialInterfaceCast = Array
		                          | ComObject
		                          | ICastable

	}

	/// <summary>
	/// Use with: ObjHeader::m_uSyncBlockValue
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

	/// <summary>
	/// Use with: MethodTable::m_dwFlags.Flags
	/// </summary>
	[Flags]
	public enum MethodTableFlagsLow : ushort
	{

		// AS YOU ADD NEW FLAGS PLEASE CONSIDER WHETHER Generics::NewInstantiation NEEDS
		// TO BE UPDATED IN ORDER TO ENSURE THAT METHODTABLES DUPLICATED FOR GENERIC INSTANTIATIONS
		// CARRY THE CORRECT FLAGS.
		//

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


		// IMPORTANT! IMPORTANT! IMPORTANT!
		//
		// As you change the flags in WFLAGS_LOW_ENUM you also need to change this
		// to be up to date to reflect the default values of those flags for the
		// case where this MethodTable is for a String or Array

		StringArrayValues = (StaticsMask_NonDynamic & 0xFFFF |
		                     NotInPZM & 0 |
		                     GenericsMask_NonGeneric & 0xFFFF |
		                     HasVariance & 0 |
		                     HasDefaultCtor & 0 |
		                     HasPreciseInitCctors & 0)


	}

	/// <summary>
	/// Source: https://github.com/dotnet/coreclr/blob/master/src/vm/methodtable.h#L4049
	/// Use with: MethodTable::m_wFlags2
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
		/// Type requires 8-byte alignment (only set on platforms that require this and don't get it implicitly)
		/// </summary>
		RequiresAlign8 = 0x1000,

		HasBoxedRegularStatics                = 0x2000,
		HasSingleNonVirtualSlot               = 0x4000,
		DependsOnEquivalentOrForwardedStructs = 0x8000

	}

	/// <summary>
	/// Source: https://github.com/dotnet/coreclr/blob/master/src/vm/class.h#L396
	/// Use with: EEClassLayoutInfo::m_bFlags
	/// </summary>
	[Flags]
	public enum LayoutFlags : byte
	{
		/// <summary>
		/// TRUE if the GC layout of the class is bit-for-bit identical
		/// to its unmanaged counterpart (i.e. no internal reference fields,
		/// no ansi-unicode char conversions required, etc.) Used to
		/// optimize marshaling.
		/// </summary>
		Blittable = 0x01,

		/// <summary>
		/// Is this type also sequential in managed memory?
		/// </summary>
		ManagedSequential = 0x02,

		/// <summary>
		/// When a sequential/explicit type has no fields, it is conceptually
		/// zero-sized, but actually is 1 byte in length. This holds onto this
		/// fact and allows us to revert the 1 byte of padding when another
		/// explicit type inherits from this type.
		/// </summary>
		ZeroSized = 0x04,

		/// <summary>
		/// The size of the struct is explicitly specified in the meta-data.
		/// </summary>
		HasExplicitSize = 0x08,

		/// <summary>
		/// Whether a native struct is passed in registers.
		/// </summary>
		NativePassInRegisters = 0x10,

		R4HFA = 0x10,
		R8HFA = 0x20,
	}

	/// <summary>
	/// Use with EEClass::VMFlags
	/// </summary>
	[Flags]
	public enum VMFlags : uint
	{
		VmflagLayoutDependsOnOtherModules = 0x00000001,
		VmflagDelegate                    = 0x00000002,
		VmflagFixedAddressVtStatics       = 0x00000020, // Value type Statics in this class will be pinned
		VmflagHaslayout                   = 0x00000040,
		VmflagIsnested                    = 0x00000080,
		VmflagIsEquivalentType            = 0x00000200,

		//   OVERLAYED is used to detect whether Equals can safely optimize to a bit-compare across the structure.
		VmflagHasoverlayedfields = 0x00000400,

		// Set this if this class or its parent have instance fields which
		// must be explicitly inited in a constructor (e.g. pointers of any
		// kind, gc or native).
		//
		// Currently this is used by the verifier when verifying value classes
		// - it's ok to use uninitialised value classes if there are no
		// pointer fields in them.
		VmflagHasFieldsWhichMustBeInited = 0x00000800,

		VmflagUnsafevaluetype = 0x00001000,

		VmflagBestfitmappingInited =
			0x00002000,                           // VMFLAG_BESTFITMAPPING and VMFLAG_THROWONUNMAPPABLECHAR are valid only if this is set
		VmflagBestfitmapping        = 0x00004000, // BestFitMappingAttribute.Value
		VmflagThrowonunmappablechar = 0x00008000, // BestFitMappingAttribute.ThrowOnUnmappableChar

		// unused                              = 0x00010000,
		VmflagNoGuid             = 0x00020000,
		VmflagHasnonpublicfields = 0x00040000,

		// unused                              = 0x00080000,
		VmflagContainsStackPtr = 0x00100000,
		VmflagPreferAlign8     = 0x00200000, // Would like to have 8-byte alignment
		// unused                              = 0x00400000,

		VmflagSparseForCominterop = 0x00800000,

		// interfaces may have a coclass attribute
		VmflagHascoclassattrib   = 0x01000000,
		VmflagComeventitfmask    = 0x02000000, // class is a special COM event interface
		VmflagProjectedFromWinrt = 0x04000000,
		VmflagExportedToWinrt    = 0x08000000,

		// This one indicates that the fields of the valuetype are
		// not tightly packed and is used to check whether we can
		// do bit-equality on value types to implement ValueType::Equals.
		// It is not valid for classes, and only matters if ContainsPointer
		// is false.
		VmflagNotTightlyPacked = 0x10000000,

		// True if methoddesc on this class have any real (non-interface) methodimpls
		VmflagContainsMethodimpls = 0x20000000,

		VmflagMarshalingtypeMask = 0xc0000000,

		VmflagMarshalingtypeInhibit      = 0x40000000,
		VmflagMarshalingtypeFreethreaded = 0x80000000,
		VmflagMarshalingtypeStandard     = 0xc0000000,

	}

}