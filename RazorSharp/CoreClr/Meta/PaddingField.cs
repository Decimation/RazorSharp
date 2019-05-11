using RazorSharp.Memory.Pointers;

namespace RazorSharp.CoreClr.Meta
{
	public class PaddingField : TransientField
	{
		internal PaddingField(int memOffset, int size) : base(memOffset, size) { }

		public override string Name => "(padding)";

		public override object GetValue(object value)
		{
			return null;
		}

		public override Pointer<byte> GetAddress<TInstance>(ref TInstance value)
		{
			throw new System.NotImplementedException();
		}

		public override string TypeName => "Padding";
	}
}