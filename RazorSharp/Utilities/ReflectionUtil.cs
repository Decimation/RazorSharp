#region

using System;
using System.Linq;
using System.Reflection;
using RazorCommon;
using RazorCommon.Diagnostics;
using RazorSharp.CoreClr;
using RazorSharp.CoreClr.Meta;
using RazorSharp.CoreClr.Structures;

#endregion

namespace RazorSharp.Utilities
{
	/// <summary>
	///     Provides utilities for working with Reflection
	/// </summary>
	public static class ReflectionUtil
	{
		public static MetaType GetMetaType(this Type t) => new MetaType(t.GetMethodTable());

		/// <summary>
		/// Executes a generic method
		/// </summary>
		/// <param name="t">Enclosing type</param>
		/// <param name="name">Method name</param>
		/// <param name="instance">Instance of type <paramref name="t"/>; <c>null</c> if the method is static</param>
		/// <param name="typeArgs">Generic type parameters</param>
		/// <param name="args">Method arguments</param>
		/// <returns>Return value of the method specified by <seealso cref="name"/></returns>
		public static object InvokeGenericMethod(Type            t,
		                                         string          name,
		                                         object          instance,
		                                         Type[]          typeArgs,
		                                         params object[] args)
		{
			var method = t.GetAnyMethod(name);
			Conditions.NotNull(method, nameof(method));


			method = method.MakeGenericMethod(typeArgs);

			return method.Invoke(method.IsStatic ? null : instance, args);
		}

		#region BindingFlags

		/// <summary>
		///     <see cref="ALL_INSTANCE_FLAGS" /> and <see cref="BindingFlags.Static" />
		/// </summary>
		public const BindingFlags ALL_FLAGS = ALL_INSTANCE_FLAGS | BindingFlags.Static;

		/// <summary>
		///     <see cref="BindingFlags.Public" />, <see cref="BindingFlags.Instance" />,
		///     and <see cref="BindingFlags.NonPublic" />
		/// </summary>
		public const BindingFlags ALL_INSTANCE_FLAGS =
			BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;

		#endregion

		#region Methods

		internal static MethodInfo[] GetAllMethods(this Type t) => t.GetMethods(ALL_FLAGS);

		internal static MethodInfo GetAnyMethod(this Type t, string name) => t.GetMethod(name, ALL_FLAGS);

		#endregion

		#region Fields

		internal static FieldInfo[] GetAllFields(this Type t) => t.GetFields(ALL_FLAGS);

		internal static FieldInfo GetAnyField(this Type t, string name) => t.GetField(name, ALL_FLAGS);

		/// <summary>
		///     Gets the corresponding <see cref="FieldInfo" />s equivalent to the fields
		///     in <see cref="MethodTable.FieldDescList" />
		/// </summary>
		internal static FieldInfo[] GetMethodTableFields(this Type t)
		{
			FieldInfo[] fields = t.GetFields(ALL_FLAGS);
			Arrays.RemoveAll(ref fields, f => f.IsLiteral);
			return fields;
		}

		#endregion
	}
}