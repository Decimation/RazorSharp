#region

using System;

#endregion

namespace RazorSharp.Utilities.Exceptions
{

	internal class TypeException : Exception
	{
		internal TypeException(string s) : base(s) { }

		private TypeException(Type expected, Type actual) : base(
			$"Expected: typeof({expected.Name}), actual: {actual.Name}") { }

		internal static void Throw<TExpected, TActual>()
		{
			throw new TypeException(typeof(TExpected), typeof(TActual));
		}
	}

}