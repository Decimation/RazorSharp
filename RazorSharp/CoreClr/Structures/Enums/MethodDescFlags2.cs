#region

using System;

#endregion

namespace RazorSharp.CoreClr.Structures.Enums
{
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
}