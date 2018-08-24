#region

#region

using System;
using System.Runtime.InteropServices;
using RazorCommon;

#endregion

// ReSharper disable ConvertToAutoPropertyWhenPossible

#endregion

namespace RazorSharp.CLR.Structures.HeapObjects
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
// 		[FieldOffset(-8) public ObjHeader _header
		[FieldOffset(0)] private          MethodTable* m_methodTablePtr;
		[FieldOffset(8)] private readonly byte         m_fields;



		/// <summary>
		/// Equal to GC_MARKED in /src/gc/gc.cpp
		/// <remarks>
		/// Source: /src/vm/object.h: 198
		/// </remarks>
		/// </summary>
		private const int MARKED_BIT = 0x1;

		/// <summary>
		/// <remarks>
		/// Address-sensitive
		/// </remarks>
		///
		/// </summary>
		public ObjHeader* Header => (ObjHeader*) (Unsafe.AddressOf(ref this) - IntPtr.Size);

		public MethodTable* MethodTable {
			get => m_methodTablePtr;
			internal set => m_methodTablePtr = value;
		}

		// object.h: 198
		// We should always use GetGCSafeMethodTable() if we're running during a GC.
		// If the mark bit is set then we're running during a GC
		public bool IsMarked {
			get { return (((ulong) (m_methodTablePtr)) & (MARKED_BIT)) == 0; }
		}


		public MethodTable* GetGCSafeMethodTable()
		{
			// lose GC marking bit and the reserved bit
			// A method table pointer should always be aligned.  During GC we set the least
			// significant bit for marked objects, and the second to least significant
			// bit is reserved.  So if we want the actual MT pointer during a GC
			// we must zero out the lowest 2 bits.
			return (MethodTable*) (((ulong) (m_methodTablePtr)) & ~((uint) 3));
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