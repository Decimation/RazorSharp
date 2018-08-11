namespace RazorSharp
{

	public static class RazorMath
	{

		public static bool Between(long num, long lower, long upper, bool inclusive = false)
		{
			return inclusive
				? lower <= num && num <= upper
				: lower < num && num < upper;
		}
	}

}