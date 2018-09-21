using System;
using RazorSharp.CLR.Structures;

namespace RazorSharp.CLR
{

	public static class Offsets
	{
		internal const int FIELD_DESC_LIST_FIELD_OFFSET = 24;
		internal const int CHUNKS_FIELD_OFFSET = 32;

		/// <summary>
		///     Heap offset to the first array element.
		///     <list type="bullet">
		///         <item>
		///             <description>+ 8 for <c>MethodTable*</c> (<see cref="IntPtr.Size" />)</description>
		///         </item>
		///         <item>
		///             <description>+ 4 for length (<see cref="UInt32" />) </description>
		///         </item>
		///         <item>
		///             <description>+ 4 for padding (<see cref="UInt32" />) (x64 only)</description>
		///         </item>
		///     </list>
		/// </summary>
		public static readonly unsafe int OffsetToArrayData = sizeof(MethodTable*) + sizeof(uint) + sizeof(uint);

		/// <summary>
		///     The offset, in bytes, of an array's actual <see cref="MethodTable" /> pointer, relative to the
		///     address pointed to by an array type's <see cref="RuntimeTypeHandle.Value" />.
		///     <para>
		///         An array's <see cref="MethodTable" /> pointer is located 1 indirection of
		///         <see cref="RuntimeTypeHandle.Value" /> + <see cref="ARRAY_MT_PTR_OFFSET" /> bytes.
		///     </para>
		/// </summary>
		internal const int ARRAY_MT_PTR_OFFSET = 6;

		/// <summary>
		///     How many bytes to subtract from <see cref="MethodTable.m_pCanonMT" /> if <see cref="MethodTable.UnionType" /> is
		///     <see cref="LowBits.MethodTable" />
		///     <remarks>
		///         <para>Source: /src/vm/methodtable.inl: 1180</para>
		///     </remarks>
		/// </summary>
		internal const int CANON_MT_UNION_MT_OFFSET = 2;
	}

}