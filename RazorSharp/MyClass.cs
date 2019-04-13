#region

using System;

#endregion

namespace RazorSharp
{
	internal class BindAttribute : Attribute { }

	public class MyClass
	{
		public int    I;
		public string S;

		public byte this[int id] => 0;

		[Bind]
		public static extern void Func();

		public override string ToString()
		{
			return String.Format("s: {0} | i : {1}", S, I);
		}
	}
}