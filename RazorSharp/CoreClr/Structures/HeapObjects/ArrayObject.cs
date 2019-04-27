#region

using System;
using System.Runtime.InteropServices;
using RazorCommon;
using RazorCommon.Strings;
using RazorSharp.Memory;

// ReSharper disable FieldCanBeMadeReadOnly.Local

// ReSharper disable ConvertToAutoProperty
// ReSharper disable ConvertToAutoPropertyWhenPossible

#endregion

namespace RazorSharp.CoreClr.Structures.HeapObjects
{
	/// <summary>
	///     <para>Represents the layout of an array in heap memory.</para>
	///     <para>Corresponding files:</para>
	///     <list type="bullet">
	///         <item>
	///             <description>/src/vm/object.h</description>
	///         </item>
	///         <item>
	///             <description>/src/vm/object.cpp</description>
	///         </item>
	///         <item>
	///             <description>/src/vm/object.inl</description>
	///         </item>
	///     </list>
	///     <para>Lines of interest:</para>
	///     <list type="bullet">
	///         <item>
	///             <description>/src/vm/object.h: 743</description>
	///         </item>
	///     </list>
	///     <remarks>
	///         Should be used with <see cref="Runtime.GetArrayObject{T}" /> and double indirection.
	///     </remarks>
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct ArrayObject : IHeapObject
	{
		#region Fields

		private MethodTable* m_methodTablePtr;

		private uint m_numComponents;

		private uint m_pad;

		#endregion

		public uint Length => m_numComponents;

		/// <summary>
		///     <remarks>
		///         Address-sensitive
		///     </remarks>
		/// </summary>
		public ObjHeader* Header => (ObjHeader*) (Unsafe.AddressOf(ref this) - IntPtr.Size);

		public MethodTable* MethodTable => m_methodTablePtr;


		public override string ToString()
		{
			var table = new ConsoleTable("Field", "Value");
			table.AddRow("Header*", Hex.ToHex(Header));
			table.AddRow("MethodTable*", Hex.ToHex(m_methodTablePtr));
			table.AddRow("Length", Length);


			return table.ToString();
		}
	}
}