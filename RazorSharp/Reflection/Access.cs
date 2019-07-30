using System;
using System.Collections.Generic;
using System.Reflection;

// ReSharper disable ReturnTypeCanBeEnumerable.Global

namespace RazorSharp.Reflection
{
	/// <summary>
	/// Provides utilities for accessing members of a type.
	/// </summary>
	internal static class Access
	{
		#region Flags

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

		#region Member

		internal static MemberInfo[] GetAllMembers(this Type t) => t.GetMembers(ALL_FLAGS);

		internal static MemberInfo[] GetAnyMember(this Type t, string name) =>
			t.GetMember(name, ALL_FLAGS);

		#endregion

		#region Field

		internal static FieldInfo GetAnyField(this Type t, string name) =>
			t.GetField(name, ALL_FLAGS);

		internal static FieldInfo[] GetAllFields(this Type t) => t.GetFields(ALL_FLAGS);

		#endregion

		#region Methods

		internal static MethodInfo[] GetAllMethods(this Type t) => t.GetMethods(ALL_FLAGS);

		internal static MethodInfo GetAnyMethod(this Type t, string name) =>
			t.GetMethod(name, (BindingFlags) ALL_FLAGS);

		#endregion

		#region Attributes

		internal static (MemberInfo, TAttribute) GetFirstAnnotated<TAttribute>(this Type t)
			where TAttribute : Attribute
		{
			var rg = t.GetAnnotated<TAttribute>();

			if (rg.Item1.Length == default || rg.Item2.Length == default) {
				return (null, null);
			}

			return (rg.Item1[0], rg.Item2[0]);
		}

		internal static (MemberInfo[], TAttribute[]) GetAnnotated<TAttribute>(this Type t)
			where TAttribute : Attribute
		{
			var members    = new List<MemberInfo>();
			var attributes = new List<TAttribute>();

			foreach (var member in t.GetAllMembers()) {
				if (Attribute.IsDefined(member, typeof(TAttribute))) {
					members.Add(member);
					attributes.Add(member.GetCustomAttribute<TAttribute>());
				}
			}

			return (members.ToArray(), attributes.ToArray());
		}

		#endregion
	}
}