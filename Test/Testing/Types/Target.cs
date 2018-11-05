namespace Test.Testing.Types
{

	public class Target
	{
		private int m_value;

		public void set(int i)
		{
			m_value = i;
		}

		public void add(int i)
		{
			m_value += i;
		}

		public int get()
		{
			return m_value;
		}

		public void sub(int i)
		{
			m_value -= i;
		}


		public int sub(int a, int b)
		{
			return a - b - m_value;
		}


		public static Target operator +(Target t, int i)
		{
			t.add(i);
			return t;
		}

	}

}