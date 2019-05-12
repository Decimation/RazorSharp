using RazorSharp.Memory;
using RazorSharp.Memory.Pointers;

namespace RazorSharp.CoreClr.Meta.Transient
{
	public class PaddingField : TransientField
	{
		internal PaddingField(int memAndFieldOffset, int size) 
			: base(memAndFieldOffset, memAndFieldOffset, size) { }
		
		public override string Name => "(Padding)";

		public override object GetValue(object value) => default(int);

		public override Pointer<byte> GetAddress<TInstance>(ref TInstance value)
		{
			var ptr = Unsafe.AddressOfData(ref value);
			return ptr + Offset;
		}

		public override string TypeName => "Padding";
	}
}