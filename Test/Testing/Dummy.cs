using System;
using System.Threading;
using RazorCommon;

namespace Test.Testing
{

	public class Dummy
	{

		private        int    _integer;
		private        string _string;
		internal       uint   Internal;
		protected      ushort Protected;
		private static int    _static;
		public         int    Public;


		public int Integer {
			get => _integer;
			set => _integer = value;
		}

		public string String {
			get => _string;
			set => _string = value;
		}

		public Dummy() : this(new Random().Next(0, 100), "foo") { }

		internal Dummy(int i, string s)
		{
			_integer = i;
			_string  = s;
		}


		public void DoSomething() { }

		public override string ToString()
		{
			return String.Format("Int: {0}, String: {1}", _integer, _string);
		}
	}


}