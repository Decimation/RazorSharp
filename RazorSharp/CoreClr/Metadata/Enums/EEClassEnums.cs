// ReSharper disable InconsistentNaming

using System;

namespace RazorSharp.CoreClr.Metadata.Enums
{
	public enum EEClassFieldId : uint
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
	
	[Flags]
	public enum VMFlags : uint
	{
		LayoutDependsOnOtherModules = 0x00000001,
		Delegate                    = 0x00000002,

		/// <summary>
		///     Value type Statics in this class will be pinned
		/// </summary>
		FixedAddressVtStatics = 0x00000020,
		HasLayout        = 0x00000040,
		IsNested         = 0x00000080,
		IsEquivalentType = 0x00000200,

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

		/// <summary>
		///     <see cref="BestFitMapping" /> and <see cref="ThrowOnUnmappableChar" /> are valid only if this is set
		/// </summary>
		BestFitMappingInited = 0x00002000,
		BestFitMapping        = 0x00004000, // BestFitMappingAttribute.Value
		ThrowOnUnmappableChar = 0x00008000, // BestFitMappingAttribute.ThrowOnUnmappableChar

		// unused                              = 0x00010000,
		NoGuid             = 0x00020000,
		HasNonPublicFields = 0x00040000,

		// unused                              = 0x00080000,
		ContainsStackPtr = 0x00100000,

		/// <summary>
		///     Would like to have 8-byte alignment
		/// </summary>
		PreferAlign8 = 0x00200000,
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
}