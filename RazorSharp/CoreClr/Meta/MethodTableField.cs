using System;
using RazorSharp.CoreClr.Structures;
using RazorSharp.Memory;
using RazorSharp.Memory.Pointers;
using SimpleSharp.Diagnostics;

namespace RazorSharp.CoreClr.Meta
{
	/// <summary>
	/// Represents the <see cref="MethodTable"/> pointer in heap memory of an object.
	/// </summary>
	public unsafe class MethodTableField : TransientField
	{
		private Pointer<MethodTable> m_value;

		internal MethodTableField() : base(0)
		{
			
		}

		public override object GetValue(object value)
		{
			Conditions.Require(!Runtime.IsStruct(value), nameof(value));
			return Runtime.ReadMethodTable(value);
		}

		public override Pointer<byte> GetAddress<TInstance>(ref TInstance value)
		{
			Conditions.Require(!Runtime.IsStruct(value), nameof(value));

			Unsafe.TryGetAddressOfHeap(value, OffsetOptions.NONE, out var ptr);

			return ptr;
		}

		public override string TypeName => nameof(MethodTable);
		
		public override string Name => "MethodTable pointer";
	}
}