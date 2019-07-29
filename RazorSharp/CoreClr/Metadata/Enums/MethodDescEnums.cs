using System;

namespace RazorSharp.CoreClr.Metadata.Enums
{
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
}