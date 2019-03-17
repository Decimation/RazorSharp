using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AnnotationUtil = RazorSharp.Utilities.AnnotationUtil;

namespace RazorSharp.Utilities
{
	internal static class AnnotationUtil
	{
		internal static MethodInfo[] GetAnnotatedMethods<TAttribute>(this Type t, string name)
			where TAttribute : Attribute
		{
			return t.GetAnnotatedMethods<TAttribute>(name, ReflectionUtil.ALL_FLAGS);
		}

		internal static MethodInfo[] GetAnnotatedMethods<TAttribute>(this Type t, string name, BindingFlags flags)
			where TAttribute : Attribute
		{
			return t.GetAnnotatedMethods<TAttribute>(flags).Where(x => x.Name == name).ToArray();
		}

		internal static MethodInfo[] GetAnnotatedMethods<TAttribute>(this Type t) where TAttribute : Attribute
		{
			return t.GetAnnotatedMethods<TAttribute>(ReflectionUtil.ALL_FLAGS);
		}

		internal static MethodInfo[] GetAnnotatedMethods<TAttribute>(this Type t, BindingFlags flags)
			where TAttribute : Attribute
		{
			MethodInfo[] methods           = t.GetMethods(flags);
			var          attributedMethods = new List<MethodInfo>();

			foreach (var t1 in methods) {
				var attr = t1.GetCustomAttribute<TAttribute>();
				if (attr != null)
					attributedMethods.Add(t1);
			}

			return attributedMethods.ToArray();
		}

		internal static (FieldInfo[], TAttribute[]) GetAnnotatedFields<TAttribute>(this Type t)
			where TAttribute : Attribute
		{
			return t.GetAnnotatedFields<TAttribute>(ReflectionUtil.ALL_FLAGS);
		}

		internal static (FieldInfo[], TAttribute[]) GetAnnotatedFields<TAttribute>(this Type t, BindingFlags flags)
			where TAttribute : Attribute
		{
			FieldInfo[] field            = t.GetFields(flags);
			var         attributedFields = new List<FieldInfo>();
			var         attributes       = new List<TAttribute>();

			foreach (var t1 in field) {
				var attr = t1.GetCustomAttribute<TAttribute>();
				if (attr != null) {
					attributedFields.Add(t1);
					attributes.Add(attr);
				}
			}

			return (attributedFields.ToArray(), attributes.ToArray());
		}
	}
}