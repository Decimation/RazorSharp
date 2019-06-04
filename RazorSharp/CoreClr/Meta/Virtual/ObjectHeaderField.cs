using System;
using RazorSharp.CoreClr.Structures;
using RazorSharp.Memory;
using RazorSharp.Memory.Pointers;
using SimpleSharp.Diagnostics;

namespace RazorSharp.CoreClr.Meta.Virtual
{
	/// <summary>
	/// Represents the <see cref="ObjHeader"/> pointer in heap memory of an object.
	/// </summary>
	public unsafe class ObjectHeaderField : VirtualField
	{
		internal ObjectHeaderField() : base(-IntPtr.Size, -IntPtr.Size*2) { }
		
		public override object GetValue(object value)
		{
			Conditions.Require(!RtInfo.IsStruct(value), nameof(value));
			return Runtime.ReadObjHeader(value);
		}

		public override Pointer<byte> GetAddress<TInstance>(ref TInstance value)
		{
			return base.GetAddress(ref value, OffsetOptions.HEADER);
		}

		public override string Name => "(Object Header)";
		
		public override string TypeName => nameof(ObjHeader);
	}
}