using System;
using System.Runtime.InteropServices;
using RazorCommon;

namespace RazorSharp.Runtime.CLRTypes.HeapObjects
{

	/// <summary>
	/// Source: https://github.com/dotnet/coreclr/blob/a6c2f7834d338e08bf3dcf9dedb48b2a0c08fcfa/src/vm/object.h#L188
	/// Should be used with Runtime.GetHeapObject and double indirection
	/// </summary>
	[StructLayout(LayoutKind.Explicit)]
	public unsafe struct HeapObject : IHeapObject
	{
		// [FieldOffset(-8) public ObjHeader _header

		[FieldOffset(0)] private          MethodTable* m_methodTablePtr;
		[FieldOffset(8)] private readonly byte         m_fields;

		public ObjHeader* Header => (ObjHeader*) (Unsafe.AddressOf(ref this) - IntPtr.Size);

		public MethodTable* MethodTable {
			get => m_methodTablePtr;
			internal set => m_methodTablePtr = value;
		}

		public override string ToString()
		{
			var table = new ConsoleTable("Field", "Value");
			table.AddRow("Header*", Hex.ToHex(Header));
			table.AddRow("MethodTable*", Hex.ToHex(m_methodTablePtr));


			return table.ToMarkDownString();
		}
	}

}