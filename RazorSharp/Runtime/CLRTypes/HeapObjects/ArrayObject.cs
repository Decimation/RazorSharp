using System;
using System.Runtime.InteropServices;
using RazorCommon;

namespace RazorSharp.Runtime.CLRTypes.HeapObjects
{

	//https://github.com/dotnet/coreclr/blob/a6c2f7834d338e08bf3dcf9dedb48b2a0c08fcfa/src/vm/object.h#L743
	[StructLayout(LayoutKind.Explicit)]
	public unsafe struct ArrayObject : IHeapObject
	{
		// [FieldOffset(-8) public ObjHeader _header

		[FieldOffset(0)]  private readonly MethodTable*      m_methodTablePtr;
		[FieldOffset(8)]  private readonly uint              m_numComponents;
		[FieldOffset(12)] private readonly uint              m_pad;
		[FieldOffset(16)] private readonly RuntimeTypeHandle m_handle;

		public uint         Length      => m_numComponents;
		public ObjHeader*   Header      => (ObjHeader*) (Unsafe.AddressOf(ref this) - IntPtr.Size);
		public MethodTable* MethodTable => m_methodTablePtr;

		/// <summary>
		/// Only present if the method table is shared among many types (arrays of pointers)
		/// </summary>
		public RuntimeTypeHandle Handle => m_handle;

		public override string ToString()
		{
			var table = new ConsoleTable("Field", "Value");
			table.AddRow("Header*", Hex.ToHex(Header));
			table.AddRow("MethodTable*", Hex.ToHex(m_methodTablePtr));
			table.AddRow("Length", Length);

			if (m_handle.Value != IntPtr.Zero) {
				table.AddRow("TypeHandle*", Hex.ToHex(m_handle.Value));
			}

			return table.ToMarkDownString();
		}
	}

}