using System;

namespace Test.Testing
{

	internal struct Point
	{

		public int X { get; }

		public int Y { get; }

		public Point(int x, int y)
		{
			X = x;
			Y = y;
		}

		public void DoSomething() {
			Console.WriteLine("hi");
		}

		public override string ToString()
		{
			return String.Format("x: {0}, y: {1}", X, Y);
		}
	}

}