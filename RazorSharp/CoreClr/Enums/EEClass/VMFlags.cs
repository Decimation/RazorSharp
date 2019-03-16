using System;
using RazorSharp.CoreClr.Structures.EE;

// ReSharper disable InconsistentNaming

namespace RazorSharp.CoreClr.Enums.EEClass {
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
}