using System;
using RazorSharp.CoreClr.Structures;
using RazorSharp.Memory;
using RazorSharp.Memory.Pointers;
using SimpleSharp.Diagnostics;

namespace RazorSharp.CoreClr.Meta.Virtual
{
	/// <summary>
	/// Represents the <see cref="MethodTable"/> pointer in heap memory of an object.
	/// </summary>
	public unsafe class MethodTableField : VirtualField
	{
		internal MethodTableField() : base(0, -IntPtr.Size) { }

		public override object GetValue(object value)
		{
			Conditions.Require(!RtInfo.IsStruct(value), nameof(value));
			return Runtime.ReadMethodTable(value);
		}

		public override Pointer<byte> GetAddress<TInstance>(ref TInstance value)
		{
			return base.GetAddress(ref value, OffsetOptions.NONE);
		}

		public override string TypeName => nameof(MethodTable);

		public override string Name => "(MethodTable pointer)";
	}
}