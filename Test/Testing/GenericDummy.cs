#region

using System;

#endregion

namespace Test.Testing
{

	internal class GenericDummy<T>
	{
		private T m_value;

		public T Value {
			get => m_value;
			set => m_value = value;
		}

		public GenericDummy()
		{
			m_value = default;
		}

		public GenericDummy(T value)
		{
			m_value = value;
		}

		public void hello() { }

		public override string ToString()
		{
			return String.Format("Value: {0}", m_value);
		}
	}

}