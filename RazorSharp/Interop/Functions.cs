#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NativeSharp.Kernel;
using RazorSharp.Import.Attributes;
using RazorSharp.Memory.Pointers;
using RazorSharp.Reflection;

// ReSharper disable SuggestBaseTypeForParameter

#endregion

// ReSharper disable ParameterTypeCanBeEnumerable.Local

namespace RazorSharp.Interop
{
	#region

	#endregion

	
	[ImportNamespace]
	internal static class Functions
	{
		/// <summary>
		///     Gets an exported function
		/// </summary>
		public static TDelegate GetExportedFunction<TDelegate>(string dllName, string fn) where TDelegate : Delegate
		{
			var hModule = Kernel32.GetModuleHandle(dllName);
			var hFn     = Kernel32.GetProcAddress(hModule, fn);
			return DelegateCreator.CreateDelegate<TDelegate>(hFn);
		}


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
			return DelegateCreator.CreateDelegate(ptr, type);
		}

		/// <summary>
		///     Calls the <c>void</c> function located at the address specified by <paramref name="ptr" /> with the
		///     arguments specified by <paramref name="args" />. No return value.
		/// </summary>
		/// <param name="ptr">Function pointer</param>
		/// <param name="args">Function arguments</param>
		public static void FluidCallVoid(Pointer<byte> ptr, params object[] args)
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
		public static object FluidCall(Pointer<byte> ptr, params object[] args) => FluidCall<object>(ptr, args);

		/// <summary>
		///     Calls the function located at the address specified by <paramref name="ptr" /> with the
		///     arguments specified by <paramref name="args" />
		/// </summary>
		/// <param name="ptr">Function pointer</param>
		/// <param name="args">Function arguments</param>
		/// <typeparam name="T">Function return type</typeparam>
		/// <returns>The value returned by the function</returns>
		public static T FluidCall<T>(Pointer<byte> ptr, params object[] args)
		{
			var d = CreateFluidCallDelegate(ptr, typeof(T), args);
			return (T) d.DynamicInvoke(args);
		}

		#endregion

		#region Generic

		/// <summary>
		///     Executes a generic method
		/// </summary>
		/// <param name="t">Enclosing type</param>
		/// <param name="name">Method name</param>
		/// <param name="value">Instance of type <paramref name="t" />; <c>null</c> if the method is static</param>
		/// <param name="typeArgs">Generic type parameters</param>
		/// <param name="args">Method arguments</param>
		/// <returns>Return value of the method specified by <seealso cref="name" /></returns>
		internal static object CallGenericMethod(Type   t,        string          name, object value,
		                                         Type[] typeArgs, params object[] args)
		{
			return t.GetAnyMethod(name).MakeGenericMethod(typeArgs).Invoke(value, args);
		}

		internal static object CallGenericMethod(MethodInfo      method, object value, 
		                                         Type[] typeArgs, params object[] args)
		{
			return method.MakeGenericMethod(typeArgs).Invoke(value, args);
		}

		#endregion
	}
}