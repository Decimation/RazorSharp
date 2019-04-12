using System;

namespace RazorSharp
{
	public class MyClass
	{
		public string s;
		public int    i;

		public byte this[int id] => 0;

		public override string ToString()
		{
			return String.Format("s: {0} | i : {1}", s, i);
		}
	}
}