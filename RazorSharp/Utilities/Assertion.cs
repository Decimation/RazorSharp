using System;
using System.Diagnostics;

namespace RazorSharp.Utilities
{

	public static class Assertion
	{
		/// <summary>
		/// Asserts that TActual is TExpected
		/// </summary>
		/// <typeparam name="TExpected">Expected type</typeparam>
		/// <typeparam name="TActual">Supplied type</typeparam>
		internal static void AssertType<TExpected, TActual>()
		{
			if (typeof(TExpected) != typeof(TActual)) {
				TypeException.Throw<TExpected, TActual>();
			}
		}

		/// <summary>
		/// Inverse of AssertType
		/// </summary>
		/// <typeparam name="TNegative">Type that TActual can't be</typeparam>
		/// <typeparam name="TActual">Supplied type</typeparam>
		public static void NegativeAssertType<TNegative, TActual>()
		{
			if (typeof(TNegative) == typeof(TActual)) {
				TypeException.Throw<TActual, TNegative>();
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