#region

using System;
using System.Runtime.CompilerServices;
using RazorSharp.CoreClr.Metadata;

#endregion

namespace RazorSharp.CoreClr
{
	/// <summary>
	///     Common runtime offsets.
	/// </summary>
	public static class Offsets
	{
		/// <summary>
		///     The offset, in bytes, of an array's actual <see cref="MethodTable" /> pointer, relative to the
		///     address pointed to by an array type's <see cref="RuntimeTypeHandle.Value" />.
		///     <para>
		///         An array's <see cref="MethodTable" /> pointer is located 1 indirection of
		///         <see cref="RuntimeTypeHandle.Value" /> + <see cref="ARRAY_MT_PTR_OFFSET" /> bytes.
		///     </para>
		///     <remarks>
		///         Relative to <see cref="RuntimeTypeHandle.Value" /> (1 indirection)
		///     </remarks>
		/// </summary>
		internal const int ARRAY_MT_PTR_OFFSET = 6;

		/// <summary>
		///     Size of the length field and first character
		///     <list type="bullet">
		///         <item>
		///             <description>+ 2: First character</description>
		///         </item>
		///         <item>
		///             <description>+ 4: String length</description>
		///         </item>
		///     </list>
		/// </summary>
		public static readonly int StringOverhead = sizeof(char) + sizeof(int);

		/// <summary>
		///     Size of the length field and padding (x64)
		/// </summary>
		public static readonly int ArrayOverhead = IntPtr.Size;

		/// <summary>
		///     Size of <see cref="TypeHandle" /> and <see cref="ObjHeader" />
		///     <list type="bullet">
		///         <item>
		///             <description>+ <see cref="IntPtr.Size" />: <see cref="ObjHeader" /></description>
		///         </item>
		///         <item>
		///             <description>+ <see cref="IntPtr.Size" />: <see cref="MethodDesc" /> pointer</description>
		///         </item>
		///     </list>
		/// </summary>
		public static readonly int ObjectOverhead = IntPtr.Size * 2;

		/// <summary>
		///     Heap offset to the first field.
		///     <list type="bullet">
		///         <item>
		///             <description>+ <see cref="IntPtr.Size" /> for <see cref="TypeHandle" /></description>
		///         </item>
		///     </list>
		/// </summary>
		public static readonly int OffsetToData = IntPtr.Size;

		/// <summary>
		///     Heap offset to the first array element.
		///     <list type="bullet">
		///         <item>
		///             <description>+ <see cref="IntPtr.Size" /> for <see cref="TypeHandle" /></description>
		///         </item>
		///         <item>
		///             <description>+ 4 for length (<see cref="UInt32" />) </description>
		///         </item>
		///         <item>
		///             <description>+ 4 for padding (<see cref="UInt32" />) (x64 only)</description>
		///         </item>
		///     </list>
		/// </summary>
		public static readonly int OffsetToArrayData = OffsetToData + ArrayOverhead;

		/// <summary>
		///     Heap offset to the first string character.
		/// </summary>
		public static readonly int OffsetToStringData = RuntimeHelpers.OffsetToStringData;

		/// <summary>
		///     <para>Minimum GC object heap size</para>
		///     <para>Sources:</para>
		///     <list type="bullet">
		///         <item>
		///             <description>/src/vm/object.h: 119</description>
		///         </item>
		///     </list>
		/// </summary>
		public static readonly int MinObjectSize = ObjectOverhead + IntPtr.Size;
	}
}