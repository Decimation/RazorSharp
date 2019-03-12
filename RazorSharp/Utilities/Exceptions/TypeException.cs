#region

using System;

#endregion

namespace RazorSharp.Utilities.Exceptions
{
	internal class TypeException : PreconditionException
	{
		internal TypeException(string s) : base(s) { }

		private static string CreateMessage(Type expected, Type actual, string msg = null)
		{
			string baseMsg = $"Expected: typeof({expected.Name}), actual: {actual.Name}";
			
			if (msg != null) {
				baseMsg += $": {msg}";
			}

			return baseMsg;
		}

		private TypeException(Type expected, Type actual) : base(CreateMessage(expected, actual)) { }

		internal static void ThrowTypesNotEqual<TExpected, TActual>()
		{
			throw new TypeException(typeof(TExpected), typeof(TActual));
		}
	}
}