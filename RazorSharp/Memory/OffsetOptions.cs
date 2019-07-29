using System;
using System.Runtime.InteropServices;
using RazorSharp.CoreClr;
using RazorSharp.CoreClr.Metadata;

namespace RazorSharp.Memory
{
	/// <summary>
	///     Offset options for <see cref="Unsafe.AddressOfHeap{T}(T,OffsetOptions)" />
	/// </summary>
	public enum OffsetOptions
	{
		/// <summary>
		///     Return the pointer offset by <c>-</c><see cref="IntPtr.Size" />,
		///     so it points to the object's <see cref="ObjHeader" />.
		/// </summary>
		HEADER,

		/// <summary>
		///     If the type is a <see cref="string" />, return the
		///     pointer offset by <see cref="Offsets.OffsetToStringData" /> so it
		///     points to the string's characters.
		///     <remarks>
		///         Note: Equal to <see cref="GCHandle.AddrOfPinnedObject" /> and <c>fixed</c>.
		///     </remarks>
		/// </summary>
		STRING_DATA,

		/// <summary>
		///     If the type is an array, return
		///     the pointer offset by <see cref="Offsets.OffsetToArrayData" /> so it points
		///     to the array's elements.
		///     <remarks>
		///         Note: Equal to <see cref="GCHandle.AddrOfPinnedObject" /> and <c>fixed</c>
		///     </remarks>
		/// </summary>
		ARRAY_DATA,

		/// <summary>
		///     If the type is a reference type, return
		///     the pointer offset by <see cref="IntPtr.Size" /> so it points
		///     to the object's fields.
		/// </summary>
		FIELDS,

		/// <summary>
		///     Don't offset the heap pointer at all, so it
		///     points to the <see cref="TypeHandle"/>
		/// </summary>
		NONE
	}
}