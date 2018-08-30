namespace RazorSharp
{

	internal static class RazorMath
	{

		internal static bool Between(long num, long lower, long upper, bool inclusive = false)
		{
			return inclusive
				? lower <= num && num <= upper
				: lower < num && num < upper;
		}
	}

}