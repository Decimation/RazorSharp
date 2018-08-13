using System;

namespace Test.Testing
{

	public struct Point
	{

		public int X { get; }

		public int Y { get; }

		public Point(int x, int y)
		{
			X = x;
			Y = y;
		}

		public override string ToString()
		{
			return String.Format("x: {0}, y: {1}", X, Y);
		}
	}

}