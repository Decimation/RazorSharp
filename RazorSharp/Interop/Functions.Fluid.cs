using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using RazorSharp.Interop.Utilities;
using RazorSharp.Memory.Pointers;

namespace RazorSharp.Interop
{
	public static partial class Functions
	{
		/// <summary>
		/// Provides methods for dynamic function invocation.
		/// </summary>
		public static class Fluid
		{
			#region FluidCall

			/// <summary>
			///     Dynamically creates a <see cref="Delegate" />.
			/// </summary>
			/// <param name="ptr">Function pointer</param>
			/// <param name="returnType">Return type of the <see cref="Delegate" /></param>
			/// <param name="args">Delegate arguments</param>
			/// <returns>A matching <see cref="Delegate" /></returns>
			private static Delegate CreateFluidCallDelegate(Pointer<byte> ptr, Type returnType, object[] args)
			{
				var argTypes = new List<Type>();
				argTypes.AddRange(args.Select(o => o.GetType()));
				argTypes.Add(returnType);

				var type = Expression.GetDelegateType(argTypes.ToArray());
				return FunctionFactory.Delegates.Create(ptr, type);
			}

			/// <summary>
			///     Calls the <c>void</c> function located at the address specified by <paramref name="ptr" /> with the
			///     arguments specified by <paramref name="args" />. No return value.
			/// </summary>
			/// <param name="ptr">Function pointer</param>
			/// <param name="args">Function arguments</param>
			public static void CallVoid(Pointer<byte> ptr, params object[] args)
			{
				var d = CreateFluidCallDelegate(ptr, typeof(void), args);
				d.DynamicInvoke(args);
			}

			/// <summary>
			///     Calls the function located at the address specified by <paramref name="ptr" /> with the
			///     arguments specified by <paramref name="args" />
			/// </summary>
			/// <param name="ptr">Function pointer</param>
			/// <param name="args">Function arguments</param>
			/// <returns>The value returned by the function as an <see cref="object" /></returns>
			public static object Call(Pointer<byte> ptr, params object[] args) => Call<object>(ptr, args);

			/// <summary>
			///     Calls the function located at the address specified by <paramref name="ptr" /> with the
			///     arguments specified by <paramref name="args" />
			/// </summary>
			/// <param name="ptr">Function pointer</param>
			/// <param name="args">Function arguments</param>
			/// <typeparam name="T">Function return type</typeparam>
			/// <returns>The value returned by the function</returns>
			public static T Call<T>(Pointer<byte> ptr, params object[] args)
			{
				var d = CreateFluidCallDelegate(ptr, typeof(T), args);
				return (T) d.DynamicInvoke(args);
			}

			#endregion
		}
	}
}