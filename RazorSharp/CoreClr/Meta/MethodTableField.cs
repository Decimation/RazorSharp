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
	public unsafe class MethodTableField : IReadWriteField
	{
		private Pointer<MethodTable> m_value;

		internal MethodTableField(Pointer<MethodTable> ptr)
		{
			m_value = ptr;
		}

		public Pointer<byte> InternalValue => m_value.Cast<byte>();

		public object GetValue(object value)
		{
			Conditions.Require(!Runtime.IsStruct(value), nameof(value));
			return Runtime.ReadMethodTable(value);
		}

		public void SetValue(object t, object value)
		{
			throw new System.NotImplementedException();
		}

		public Pointer<byte> GetAddress<TInstance>(ref TInstance t)
		{
			Conditions.Require(!Runtime.IsStruct(t), nameof(t));

			Unsafe.TryGetAddressOfHeap(t, OffsetOptions.NONE, out var ptr);

			return ptr;
		}

		public int Offset {
			get => 0;
			set { throw new InvalidOperationException(); }
		}

		public int Size => sizeof(MethodTable);

		public int Token => Constants.INVALID_VALUE;

		public string Name => nameof(MethodTable);
	}
}