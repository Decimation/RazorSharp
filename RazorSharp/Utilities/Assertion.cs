using System;
using System.Diagnostics;
using System.Runtime.ExceptionServices;

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

		[HandleProcessCorruptedStateExceptions]
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

		[HandleProcessCorruptedStateExceptions]
		public static bool Throws<TException1, TException2>(Action action) where TException1 : Exception where TException2 : Exception
		{
			try {
				action();
			}
			catch (TException1) {
				return true;
			}
			catch (TException2) {
				return true;
			}

			return false;
		}
	}

}