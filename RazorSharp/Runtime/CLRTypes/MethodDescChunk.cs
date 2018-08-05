using System.Runtime.InteropServices;
using RazorCommon;

namespace RazorSharp.Runtime.CLRTypes
{

	//todo: verify
	/// <summary>
	/// Source: https://github.com/dotnet/coreclr/blob/master/src/vm/method.hpp#L1949
	/// </summary>
	[StructLayout(LayoutKind.Explicit)]
	public unsafe struct MethodDescChunk
	{
		[FieldOffset(0)] private readonly MethodTable*     m_methodTable;
		[FieldOffset(8)] private readonly MethodDescChunk* m_next;

		/// <summary>
		/// The size of this chunk minus 1 (in multiples of MethodDesc::ALIGNMENT)
		/// </summary>
		[FieldOffset(16)] private readonly byte m_size;

		/// <summary>
		/// The number of MethodDescs in this chunk minus 1
		/// </summary>
		[FieldOffset(17)] private readonly byte m_count;

		[FieldOffset(18)] private readonly ushort m_flagsAndTokenRange;

		// wtf? Why do I need to cast lol
		// m_count is a byte??
		public byte Count => (byte) (m_count + 1);

		public MethodTable* MethodTable => m_methodTable;


		public override string ToString()
		{
			var table = new ConsoleTable("Field", "Value");
			table.AddRow("MethodTable", Hex.ToHex(m_methodTable));
			table.AddRow("Next chunk", Hex.ToHex(m_next));
			table.AddRow("Size", m_size);
			table.AddRow("Count (-1)", m_count);
			table.AddRow("Flags and token range", m_flagsAndTokenRange);

			return table.ToMarkDownString();
		}
	}

}