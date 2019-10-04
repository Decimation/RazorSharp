using RazorSharp.CoreClr.Meta;
using RazorSharp.Memory.Pointers;

namespace RazorSharp.Memory.Components
{
	public class MemoryField : IStructure
	{
		public virtual string Name { get; }

		public virtual int Offset { get; }

		public virtual int Size { get; }

		internal MemoryField(string name, int offset, int size) : this(size)
		{
			Name   = name;
			Offset = offset;
		}

		protected MemoryField(int size)
		{
			Size = size;
		}

		public Pointer<byte> GetAddress<T>(ref T value)
		{
			return Unsafe.AddressOfFields(ref value).Cast() + Offset;
		}

		public virtual object GetValue(object value)
		{
			
			MetaType t = value.GetType();
			// GetElementType doesn't work for some reason
			
			if (t.IsArray) {
				t = t.ElementTypeHandle;
			}

			var ptr = GetAddress(ref value);
			

			return ptr.ReadAny(t.RuntimeType);
		}


		public override string ToString()
		{
			return base.ToString();
		}
	}
}