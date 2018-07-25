using System;
using System.Collections.Generic;
using System.Linq;

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

		internal static readonly int MIN_OBJECT_SIZE = (2 * sizeof(byte*) + sizeof(ObjHeader));

		//todo
		internal const uint GC_MARKED = 0x1;
	}

	[Flags]
	public enum MethodTableFlags : uint
	{
		// DO NOT use flags that have bits set in the low 2 bytes.
		// These flags are DWORD sized so that our atomic masking
		// operations can operate on the entire 4-byte aligned DWORD
		// instead of the logical non-aligned WORD of flags.  The
		// low WORD of flags is reserved for the component size.

		// The following bits describe mutually exclusive locations of the type
		// in the type hierarchy.
		Mask = 0x000F0000,

		Class   = 0x00000000,
		Unused1 = 0x00010000,

		MarshalByRefMask = 0x000E0000,
		MarshalByRef     = 0x00020000,
		Contextful       = 0x00030000, // sub-category of MarshalByRef

		ValueType          = 0x00040000,
		ValueTypeMask      = 0x000C0000,
		Nullable           = 0x00050000, // sub-category of ValueType
		PrimitiveValueType = 0x00060000, // sub-category of ValueType, Enum or primitive value type
		TruePrimitive      = 0x00070000, // sub-category of ValueType, Primitive (ELEMENT_TYPE_I, etc.)

		Array     = 0x00080000,
		ArrayMask = 0x000C0000,

		// IfArrayThenUnused                 = 0x00010000, // sub-category of Array
		IfArrayThenSzArray = 0x00020000, // sub-category of Array

		Interface        = 0x000C0000,
		Unused2          = 0x000D0000,
		TransparentProxy = 0x000E0000,
		AsyncPin         = 0x000F0000,

		ElementTypeMask = 0x000E0000, // bits that matter for element type mask


		HasFinalizer = 0x00100000, // instances require finalization

		IfNotInterfaceThenMarshalable = 0x00200000, // Is this type marshalable by the pinvoke marshalling layer

//#ifdef FEATURE_COMINTEROP
		IfInterfaceThenHasGuidInfo = 0x00200000, // Does the type has optional GuidInfo
//#endif // FEATURE_COMINTEROP

		ICastable = 0x00400000, // class implements ICastable interface

		HasIndirectParent = 0x00800000, // m_pParentMethodTable has double indirection

		ContainsPointers = 0x01000000,

		HasTypeEquivalence = 0x02000000, // can be equivalent to another type

//#ifdef FEATURE_COMINTEROP
		HasRCWPerTypeData = 0x04000000, // has optional pointer to RCWPerTypeData
//#endif // FEATURE_COMINTEROP

		HasCriticalFinalizer     = 0x08000000, // finalizer must be run on Appdomain Unload
		Collectible              = 0x10000000,
		ContainsGenericVariables = 0x20000000, // we cache this flag to help detect these efficiently and
		// to detect this condition when restoring

		ComObject = 0x40000000, // class is a com object

		HasComponentSize = 0x80000000, // This is set if component size is used for flags.

		// Types that require non-trivial interface cast have this bit set in the category
		NonTrivialInterfaceCast = Array
		                          | ComObject
		                          | ICastable

	}

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

	//todo: implement
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

	[Flags]
	public enum MethodTableFlags2 : ushort
	{

		// AS YOU ADD NEW FLAGS PLEASE CONSIDER WHETHER Generics::NewInstantiation NEEDS
		// TO BE UPDATED IN ORDER TO ENSURE THAT METHODTABLES DUPLICATED FOR GENERIC INSTANTIATIONS
		// CARRY THE CORECT FLAGS.

		// The following bits describe usage of optional slots. They have to stay
		// together because of we index using them into offset arrays.
		MultipurposeSlotsMask = 0x001F,
		HasPerInstInfo        = 0x0001,
		HasInterfaceMap       = 0x0002,
		HasDispatchMapSlot    = 0x0004,
		HasNonVirtualSlots    = 0x0008,
		HasModuleOverride     = 0x0010,

		IsZapped = 0x0020, // This could be fetched from m_pLoaderModule if we run out of flags

		IsPreRestored = 0x0040, // Class does not need restore
		// This flag is set only for NGENed classes (IsZapped is true)

		HasModuleDependencies = 0x0080,

		IsIntrinsicType = 0x0100,

		RequiresDispatchTokenFat = 0x0200,

		HasCctor       = 0x0400,
		HasCCWTemplate = 0x0800, // Has an extra field pointing to a CCW template

//#ifdef FEATURE_64BIT_ALIGNMENT
		RequiresAlign8 =
			0x1000, // Type requires 8-byte alignment (only set on platforms that require this and don't get it implicitly)
//#endif

		HasBoxedRegularStatics = 0x2000, // GetNumBoxedRegularStatics() != 0

		HasSingleNonVirtualSlot = 0x4000,

		DependsOnEquivalentOrForwardedStructs =
			0x8000, // Declares methods that have type equivalent or type forwarded structures in their signature

	}

}