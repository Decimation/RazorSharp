#region

using System;
using RazorSharp.CoreClr.Structures;

#endregion

namespace RazorSharp.CoreClr.Enums.MethodTable
{
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
}