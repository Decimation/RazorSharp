#region

#region

using System;
using System.Runtime.InteropServices;
using RazorCommon;

#endregion

// ReSharper disable ConvertToAutoPropertyWhenPossible

#endregion

namespace RazorSharp.Runtime.CLRTypes.HeapObjects
{


	/// <summary>
	///     <para>Represents the base layout of <see cref="object" /> in heap memory.</para>
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
	///             <description>/src/vm/object.h: 188</description>
	///         </item>
	///         <item>
	///             <description>/src/vm/object.h: 138: Layout info</description>
	///         </item>
	///     </list>
	///     <remarks>
	///         Should be used with <see cref="Runtime.GetHeapObject{T}" /> and double indirection.
	///     </remarks>
	/// </summary>
	[StructLayout(LayoutKind.Explicit)]
	public unsafe struct HeapObject : IHeapObject
	{
		// [FieldOffset(-8) public ObjHeader _header

		[FieldOffset(0)] private          MethodTable* m_methodTablePtr;
		[FieldOffset(8)] private readonly byte         m_fields;

		/// <summary>
		///     Address-sensitive
		/// </summary>
		public ObjHeader* Header => (ObjHeader*) (Unsafe.AddressOf(ref this) - IntPtr.Size);

		public MethodTable* MethodTable {
			get => m_methodTablePtr;
			internal set => m_methodTablePtr = value;
		}

		public override string ToString()
		{
			ConsoleTable table = new ConsoleTable("Field", "Value");
			table.AddRow("Header*", Hex.ToHex(Header));
			table.AddRow("MethodTable*", Hex.ToHex(m_methodTablePtr));


			return table.ToMarkDownString();
		}
	}

}