using System;
using System.Diagnostics;

namespace RazorSharp.Utilities
{

	public static class Assertion
	{
		internal static void AssertType<TExpected, TActual>()
		{
			if (typeof(TExpected) != typeof(TActual)) {
				TypeException.Throw<TExpected, TActual>();
			}
		}

		public static void AssertNoThrows<TException>(Action action) where TException : Exception
		{
			Debug.Assert(!Throws<TException>(action));
		}

		public static void AssertThrows<TException>(Action action) where TException : Exception
		{
			Debug.Assert(Throws<TException>(action));
		}

		public static bool Throws<TException>(Action action) where TException : Exception
		{
			try {
				action();
			}
			catch (TException) {
				return true;
			}

			return false;
		}
	}

}