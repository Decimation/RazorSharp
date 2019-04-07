#region

using System;

#endregion

namespace RazorSharp.CoreClr.Structures
{
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
}