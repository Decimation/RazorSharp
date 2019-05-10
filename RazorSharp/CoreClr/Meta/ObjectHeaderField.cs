using System;
using RazorSharp.CoreClr.Structures;
using RazorSharp.Memory;
using RazorSharp.Memory.Pointers;
using SimpleSharp.Diagnostics;

namespace RazorSharp.CoreClr.Meta
{
	/// <summary>
	/// Represents the <see cref="ObjHeader"/> pointer in heap memory of an object.
	/// </summary>
	public unsafe class ObjectHeaderField : IReadWriteField
	{
		private Pointer<ObjHeader> m_value;

		internal ObjectHeaderField(Pointer<ObjHeader> ptr)
		{
			m_value = ptr;
		}
		
		public int Token => Constants.INVALID_VALUE;
		public string Name => nameof(ObjHeader);
		public Pointer<byte> InternalValue => m_value.Cast<byte>();

		public object GetValue(object value)
		{
			Conditions.Require(!Runtime.IsStruct(value), nameof(value));
			return Runtime.ReadObjHeader(value);
		}

		public void SetValue(object t, object value)
		{
			throw new System.NotImplementedException();
		}

		public Pointer<byte> GetAddress<TInstance>(ref TInstance t)
		{
			Conditions.Require(!Runtime.IsStruct(t), nameof(t));

			Unsafe.TryGetAddressOfHeap(t, OffsetOptions.HEADER, out var ptr);

			return ptr;
		}

		public int Offset {
			get => -IntPtr.Size;
			set { throw new InvalidOperationException(); }
		}

		public int Size => sizeof(ObjHeader);
	}
}