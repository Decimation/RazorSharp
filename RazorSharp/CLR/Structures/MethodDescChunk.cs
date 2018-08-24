#region

using System.Runtime.InteropServices;
using RazorCommon;

#endregion

namespace RazorSharp.CLR.Structures
{

	//todo: verify
	/// <summary>
	///     <para>Corresponding files:</para>
	///     <list type="bullet">
	///         <item>
	///             <description>/src/vm/method.hpp</description>
	///         </item>
	///         <item>
	///             <description>/src/vm/method.cpp</description>
	///         </item>
	///         <item>
	///             <description>/src/vm/method.inl</description>
	///         </item>
	///     </list>
	///     <para>Lines of interest:</para>
	///     <list type="bullet">
	///         <item>
	///             <description>/src/vm/method.hpp: 1949</description>
	///         </item>
	///     </list>
	/// </summary>
	[StructLayout(LayoutKind.Explicit)]
	public unsafe struct MethodDescChunk
	{

		#region Fields

		[FieldOffset(0)]  private readonly MethodTable*     m_methodTable;
		[FieldOffset(8)]  private readonly MethodDescChunk* m_next;
		[FieldOffset(16)] private readonly byte             m_size;
		[FieldOffset(17)] private readonly byte             m_count;
		[FieldOffset(18)] private readonly ushort           m_flagsAndTokenRange;

		#endregion


		/// <summary>
		///     The size of this chunk minus 1 (in multiples of <see cref="MethodDesc.ALIGNMENT" />)
		/// </summary>
		public byte Size => m_size;

		/// <summary>
		///     The number of <see cref="MethodDesc" />s in this chunk minus 1
		/// </summary>
		public byte Count => m_count; //(byte) (m_count + 1);

		public MethodTable* MethodTable => m_methodTable;


		public override string ToString()
		{
			ConsoleTable table = new ConsoleTable("Field", "Value");
			table.AddRow("MethodTable", Hex.ToHex(m_methodTable));
			table.AddRow("Next chunk", Hex.ToHex(m_next));
			table.AddRow("Size", m_size);
			table.AddRow("Count (-1)", m_count);
			table.AddRow("Flags and token range", m_flagsAndTokenRange);

			return table.ToMarkDownString();
		}
	}

}