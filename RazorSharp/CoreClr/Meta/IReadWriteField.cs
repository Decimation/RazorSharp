using RazorSharp.Memory.Pointers;

namespace RazorSharp.CoreClr.Meta
{
	public interface IReadWriteField : IMeta
	{
		Pointer<byte> InternalValue { get; }
		
		object GetValue(object value);
		void   SetValue(object t, object value);

		Pointer<byte> GetAddress<TInstance>(ref TInstance t);

		int Offset { get; set; }
		int Size   { get; }
	}
}