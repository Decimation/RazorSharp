using System;

namespace RazorSharp
{
	public class Miscellaneous
	{
		class Attr : Attribute
		{
			public Attr() { }
		}

		[Attr]
		public static extern void Run();
	}
}