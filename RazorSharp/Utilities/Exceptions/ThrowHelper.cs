using System;
using JetBrains.Annotations;
using RazorCommon;


namespace RazorSharp.Utilities.Exceptions
{
	internal static class ThrowHelper
	{
		private static Type[] GetTypeArgs(params object[] args)
		{
			var typeArgs = new Type[args.Length];
			for (int i = 0; i < args.Length; i++) {
				typeArgs[i] = args[i].GetType();
			}

			return typeArgs;
		}

		private static TException Create<TException>(params object[] args) where TException : Exception
		{
			return Create<TException>(GetTypeArgs(args), args);
		}

		private static TException Create<TException>(Type[] typeArgs, params object[] args) where TException : Exception
		{
			Type[] typeArgsFinal = typeArgs == null || typeArgs.Length == 0 ? Type.EmptyTypes : typeArgs;

			var ctor = typeof(TException).GetConstructor(typeArgsFinal);
			Conditions.RequiresNotNull(ctor, nameof(ctor));
			return (TException) ctor.Invoke(args);
		}


		[StringFormatMethod(Conditions.STRING_FORMAT_PARAM)]
		private static TException CreateException<TException>(string msg, params object[] args)
			where TException : Exception
		{
			TException exception;
			if (!String.IsNullOrWhiteSpace(msg)) {
				string format = String.Format(msg, args);
				exception = Create<TException>(format);
			}
			else {
				exception = Create<TException>();
			}

			return exception;
		}

		[StringFormatMethod(Conditions.STRING_FORMAT_PARAM)]
		internal static void FailRequire(string msg, params object[] args)
		{
			throw CreateException<Exception>(msg, args);
		}

		[StringFormatMethod(Conditions.STRING_FORMAT_PARAM)]
		internal static void FailRequire<TException>(string msg, params object[] args) where TException : Exception
		{
			
			throw CreateException<TException>(msg, args);
		}
	}
}