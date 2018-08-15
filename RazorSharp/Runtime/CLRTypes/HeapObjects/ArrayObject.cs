#region

using System;
using System.Runtime.InteropServices;
using RazorCommon;

#endregion

namespace RazorSharp.Runtime.CLRTypes.HeapObjects
{

	/// <summary>
	/// Source: https://github.com/dotnet/coreclr/blob/a6c2f7834d338e08bf3dcf9dedb48b2a0c08fcfa/src/vm/object.h#L743
	/// Should be used with Runtime.GetArrayObject and double indirection
	/// </summary>
	[StructLayout(LayoutKind.Explicit)]
	public unsafe struct ArrayObject : IHeapObject
	{
		// [FieldOffset(-8) public ObjHeader _header

		[FieldOffset(0)]  private readonly MethodTable* m_methodTablePtr;
		[FieldOffset(8)]  private readonly uint         m_numComponents;
		[FieldOffset(12)] private readonly uint         m_pad;


		public uint Length => m_numComponents;

		/// <summary>
		/// Address-sensitive
		/// </summary>
		public ObjHeader* Header => (ObjHeader*) (Unsafe.AddressOf(ref this) - IntPtr.Size);

		public MethodTable* MethodTable => m_methodTablePtr;

		/// <summary>
		/// Only present if the method table is shared among many types (arrays of pointers)
		/// </summary>

		//public RuntimeTypeHandle Handle => m_handle;
		public override string ToString()
		{
			var table = new ConsoleTable("Field", "Value");
			table.AddRow("Header*", Hex.ToHex(Header));
			table.AddRow("MethodTable*", Hex.ToHex(m_methodTablePtr));
			table.AddRow("Length", Length);


			return table.ToMarkDownString();
		}
	}

}