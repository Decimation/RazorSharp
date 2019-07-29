using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RazorSharp.CoreClr;
using RazorSharp.CoreClr.Meta;
using SimpleSharp;

// ReSharper disable ReturnTypeCanBeEnumerable.Global

namespace RazorSharp.Reflection
{
	/// <summary>
	///     Provides utilities for working with Reflection
	/// </summary>
	internal static class ReflectionUtil
	{
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
		///     in <see cref="MetaType.FieldList" /> (<see cref="Runtime.ReadFieldDescs"/>)
		/// </summary>
		internal static FieldInfo[] GetCorrespondingMethodTableFields(Type t)
		{
			FieldInfo[] fields = t.GetFields(ALL_FLAGS);
			Arrays.RemoveAll(ref fields, f => f.IsLiteral);
			return fields;
		}

		#endregion

		#region Member

		internal static MemberInfo[] GetAllMembers(this Type t) => t.GetMembers(ALL_FLAGS);

		internal static MemberInfo[] GetAnyMember(this Type t, string name) => t.GetMember(name, ALL_FLAGS);

		#endregion

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

		/// <summary>
		///     Runs a constructor whose parameters match <paramref name="args" />
		/// </summary>
		/// <param name="value">Instance</param>
		/// <param name="args">Constructor arguments</param>
		/// <returns>
		///     <c>true</c> if a matching constructor was found and executed;
		///     <c>false</c> if a constructor couldn't be found
		/// </returns>
		internal static bool RunConstructor<T>(T value, params object[] args)
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