#region

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using RazorCommon;
// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable ConvertToAutoProperty

#endregion

namespace RazorSharp.Runtime.CLRTypes.HeapObjects
{

	#region

	using CSUnsafe = System.Runtime.CompilerServices.Unsafe;

	#endregion


	/// <summary>
	/// Source: https://github.com/dotnet/coreclr/blob/a6c2f7834d338e08bf3dcf9dedb48b2a0c08fcfa/src/vm/object.h#L1082
	/// Should be used with Runtime.GetStringObject and double indirection
	/// </summary>
	[StructLayout(LayoutKind.Explicit)]
	public unsafe struct StringObject : IHeapObject
	{
		// [FieldOffset(-8) public ObjHeader _header

		[FieldOffset(0)]  private readonly MethodTable* m_methodTablePtr;
		[FieldOffset(8)]  private readonly uint         m_stringLength;
		[FieldOffset(12)] private readonly char         m_firstChar;


		public uint Length    => m_stringLength;
		public char FirstChar => m_firstChar;

		/// <summary>
		/// Address-sensitive
		/// </summary>
		public ObjHeader* Header => (ObjHeader*) (Unsafe.AddressOf(ref this) - IntPtr.Size);

		public MethodTable* MethodTable => m_methodTablePtr;

		/// <summary>
		/// Address-sensitive
		/// </summary>

		public char this[int index] {
			get {
				var __this = (char*) Unsafe.AddressOf(ref this);

				return __this[index + (RuntimeHelpers.OffsetToStringData / 2)];
			}
		}


		public override string ToString()
		{
			var table = new ConsoleTable("Field", "Value");
			table.AddRow("Header*", Hex.ToHex(Header));
			table.AddRow("MethodTable*", Hex.ToHex(m_methodTablePtr));
			table.AddRow("Length", Length);
			table.AddRow("First char", FirstChar);

			return table.ToMarkDownString();
		}
	}

}