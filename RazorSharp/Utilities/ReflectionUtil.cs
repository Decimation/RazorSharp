using System;
using System.Collections.Generic;
using System.Reflection;
using RazorCommon;
using RazorSharp.CoreClr.Structures;

namespace RazorSharp.Utilities
{
	/// <summary>
	/// Provides utilities for working with Reflection
	/// </summary>
	public static class ReflectionUtil
	{
		#region BindingFlags

		/// <summary>
		/// <see cref="ALL_INSTANCE_FLAGS"/> and <see cref="BindingFlags.Static"/>
		/// </summary>
		public const BindingFlags ALL_FLAGS = ALL_INSTANCE_FLAGS | BindingFlags.Static;

		/// <summary>
		/// <see cref="BindingFlags.Public"/>, <see cref="BindingFlags.Instance"/>,
		/// and <see cref="BindingFlags.NonPublic"/>
		/// </summary>
		public const BindingFlags ALL_INSTANCE_FLAGS =
			BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;

		#endregion

		public static object InvokeGenericMethod(Type            t,
		                                         string          name,
		                                         Type            typeArgs,
		                                         object          instance,
		                                         params object[] args)
		{
			var method = t.GetAnyMethod(name);
			Conditions.RequiresNotNull(method, nameof(method));


			method = method.MakeGenericMethod(typeArgs);
			return method.Invoke(method.IsStatic ? null : instance, args);
		}

		#region Methods

		internal static MethodInfo[] GetAllMethods(this Type t)
		{
			return t.GetMethods(ALL_FLAGS);
		}

		internal static MethodInfo GetAnyMethod(this Type t, string name)
		{
			return t.GetMethod(name, ALL_FLAGS);
		}

		#endregion

		#region Fields

		internal static FieldInfo[] GetAllFields(this Type t)
		{
			return t.GetFields(ALL_FLAGS);
		}

		internal static FieldInfo GetAnyField(this Type t, string name)
		{
			return t.GetField(name, ALL_FLAGS);
		}

		/// <summary>
		///     Gets the corresponding <see cref="FieldInfo" />s equivalent to the fields
		///     in <see cref="MethodTable.FieldDescList" />
		/// </summary>
		internal static FieldInfo[] GetMethodTableFields(this Type t)
		{
			FieldInfo[] fields = t.GetFields(ALL_FLAGS);
			Collections.RemoveAll(ref fields, f => f.IsLiteral);
			return fields;
		}

		internal static FieldInfo[] GetMethodTableFields<T>()
		{
			return typeof(T).GetMethodTableFields();
		}

		#endregion
	}
}