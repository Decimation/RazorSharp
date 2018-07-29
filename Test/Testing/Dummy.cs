using System;

namespace Test.Testing
{

	internal class Dummy
	{
		private int    _integer;
		private string _string;

		internal Dummy() : this(new Random().Next(0, 100), "foo") { }

		internal Dummy(int i, string s)
		{
			_integer = i;
			_string  = s;
		}

		public int get()
		{
			return _integer;
		}
	}



}