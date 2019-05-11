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
	public unsafe class ObjectHeaderField : TransientField
	{
		internal ObjectHeaderField() : base(-IntPtr.Size) { }
		
		public override object GetValue(object value)
		{
			Conditions.Require(!Runtime.IsStruct(value), nameof(value));
			return Runtime.ReadObjHeader(value);
		}

		public override Pointer<byte> GetAddress<TInstance>(ref TInstance value)
		{
			Conditions.Require(!Runtime.IsStruct(value), nameof(value));

			Unsafe.TryGetAddressOfHeap(value, OffsetOptions.HEADER, out var ptr);

			return ptr;
		}

		public override string Name => "Object Header";
		
		public override string TypeName => nameof(ObjHeader);
	}
}