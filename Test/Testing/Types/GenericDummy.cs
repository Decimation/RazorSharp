#region

#endregion

namespace Test.Testing.Types
{
	internal class GenericDummy<T>
	{
		public GenericDummy()
		{
			Value = default;
		}

		public GenericDummy(T value)
		{
			Value = value;
		}

		public T Value { get; set; }

		public void hello() { }

		public override string ToString()
		{
			return string.Format("Value: {0}", Value);
		}
	}
}