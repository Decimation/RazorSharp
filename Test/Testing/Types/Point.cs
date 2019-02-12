#region

#region

using System;

#endregion

#pragma warning disable 649

#endregion

namespace Test.Testing.Types
{
	internal unsafe struct Point
	{
		public int X { get; set; }

		public int Y { get; set; }

		public fixed byte FixedBuffer[256];

		public Point(int x, int y)
		{
			X = x;
			Y = y;
		}

		public void DoSomething()
		{
			Console.WriteLine("hi");
		}

		public int getInt32()
		{
			return X + 1;
		}

		public static Point operator ++(Point p)
		{
			p.X++;
			p.Y++;
			return p;
		}

		public override string ToString()
		{
			return string.Format("x: {0}, y: {1}", X, Y);
		}
	}
}