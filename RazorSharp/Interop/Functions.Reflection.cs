using System;
using System.Linq;
using System.Reflection;

namespace RazorSharp.Interop
{
	public static partial class Functions
	{
		/// <summary>
		/// Provides methods for invoking managed methods via Reflection.
		/// </summary>
		public static class Reflection
		{
			#region Generic

			/// <summary>
			///     Executes a generic method
			/// </summary>
			/// <param name="method">Method to execute</param>
			/// <param name="typeArgs">Generic type parameters</param>
			/// <param name="value">Instance of type; <c>null</c> if the method is static</param>
			/// <param name="args">Method arguments</param>
			/// <returns>Return value of the method specified by <paramref name="method"/></returns>
			public static object CallGeneric(MethodInfo      method,
			                                 Type[]          typeArgs,
			                                 object          value,
			                                 params object[] args)
			{
				return method.MakeGenericMethod(typeArgs).Invoke(value, args);
			}

			public static object CallGeneric(MethodInfo      method,
			                                 Type            typeArg,
			                                 object          value,
			                                 params object[] args)
			{
				return method.MakeGenericMethod(typeArg).Invoke(value, args);
			}

			#endregion

			/// <summary>
			///     Runs a constructor whose parameters match <paramref name="args" />
			/// </summary>
			/// <param name="value">Instance</param>
			/// <param name="args">Constructor arguments</param>
			/// <returns>
			///     <c>true</c> if a matching constructor was found and executed;
			///     <c>false</c> if a constructor couldn't be found
			/// </returns>
			public static bool CallConstructor<T>(T value, params object[] args)
			{
				ConstructorInfo[] ctors    = value.GetType().GetConstructors();
				Type[]            argTypes = args.Select(x => x.GetType()).ToArray();

				foreach (var ctor in ctors) {
					ParameterInfo[] paramz = ctor.GetParameters();

					if (paramz.Length == args.Length) {
						if (paramz.Select(x => x.ParameterType).SequenceEqual(argTypes)) {
							ctor.Invoke(value, args);
							return true;
						}
					}
				}

				return false;
			}
		}
	}
}