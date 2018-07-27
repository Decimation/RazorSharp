namespace RazorSharp.Pointers
{
	using CSUnsafe = System.Runtime.CompilerServices.Unsafe;

	public unsafe struct LightPointer<T>
	{
		private void* m_value;

		public T Value {
			get => CSUnsafe.Read<T>(m_value);
			set => CSUnsafe.Write(m_value, value);
		}

		public override string ToString()
		{
			return Value.ToString();
		}
	}

}