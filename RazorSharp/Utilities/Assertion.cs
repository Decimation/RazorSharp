#region

using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ExceptionServices;
using RazorCommon;

#endregion

namespace RazorSharp.Utilities
{

	internal static class Assertion
	{
		internal const string WIPString = "(wip)";

		/// <summary>
		///     Asserts that TActual is TExpected
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
		///     Inverse of AssertType
		/// </summary>
		/// <typeparam name="TNegative">Type that TActual can't be</typeparam>
		/// <typeparam name="TActual">Supplied type</typeparam>
		private static void NegativeAssertType<TNegative, TActual>()
		{
			if (typeof(TNegative) == typeof(TActual)) {
				TypeException.Throw<TActual, TNegative>();
			}
		}

		internal static void AssertNoThrows<TException>(Action action) where TException : Exception
		{
			Debug.Assert(!Throws<TException>(action));
		}

		internal static void AssertThrows<TException>(Action action) where TException : Exception
		{
			Debug.Assert(Throws<TException>(action));
		}

		[HandleProcessCorruptedStateExceptions]
		internal static bool Throws<TException>(Action action) where TException : Exception
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
		internal static bool Throws<TException1, TException2>(Action action)
			where TException1 : Exception where TException2 : Exception
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

		internal static void WeakAssertEqual<T>(params T[] values)
		{
			if (values == null || values.Length == 0) {
				return;
			}

			if (!values.All(v => v.Equals(values[0]))) {
				Logger.Log(Level.Warning, Flags.Debug,
					"Equality assertion of {0} failed", Collections.ToString(values));
			}
		}

		internal static void AssertEqual<T>(params T[] values)
		{
			if (values == null || values.Length == 0) {
				return;
			}

			Debug.Assert(values.All(v => v.Equals(values[0])));
		}
	}

}