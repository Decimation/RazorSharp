#region

using System;

#endregion

// ReSharper disable InconsistentNaming

namespace RazorSharp.CoreClr.Structures
{
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
}