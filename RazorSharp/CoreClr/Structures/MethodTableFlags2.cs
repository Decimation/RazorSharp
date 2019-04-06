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
}