#region

using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ExceptionServices;
using RazorSharp.CLR;

#endregion

namespace RazorSharp.Utilities
{

	internal static class Assertion
	{


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

		internal static void AssertNoThrows<TException>(Action action) where TException : Exception
		{
			Trace.Assert(!Throws<TException>(action));
		}

		internal static void AssertThrows<TException>(Action action) where TException : Exception
		{
			Trace.Assert(Throws<TException>(action));
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

		internal static void AssertEqual<T>(params T[] values)
		{
			if (values == null || values.Length == 0) {
				return;
			}

			Trace.Assert(values.All(v => v.Equals(values[0])));
		}

		internal static void AssertFieldDescAddress(IntPtr __this)
		{
			if (!Runtime.FieldAddrMap.ContainsKey(__this)) {
				throw new RuntimeException("FieldDesc* has incorrect address. Is the FieldDesc* dereferenced?");
			}
		}

		internal static void AssertMethodDescAddress(IntPtr __this)
		{
			if (!Runtime.MethodAddrMap.ContainsKey(__this)) {
				throw new RuntimeException("MethodDesc* has incorrect address. Is the MethodDesc* dereferenced?");
			}
		}
	}

}