#region

using System;
using System.Collections.Generic;
using System.Reflection;

#endregion

namespace RazorSharp.Utilities
{
	internal static class AnnotationUtil
	{
		private static (TType[], TAttribute[]) GetAnnotated<TType, TAttribute>(Func<BindingFlags, TType[]> values,
		                                                                       BindingFlags                flags,
		                                                                       Func<TType, TAttribute>     getValue)
			where TAttribute : Attribute
		{
			TType[] units           = values(flags);
			var     attributedUnits = new List<TType>();
			var     attributes      = new List<TAttribute>();

			foreach (var unit in units) {
				var attr = getValue(unit);
				if (attr != null) {
					attributedUnits.Add(unit);
					attributes.Add(attr);
				}
			}

			return (attributedUnits.ToArray(), attributes.ToArray());
		}

		#region Methods

		internal static (MethodInfo[], TAttribute[]) GetAnnotatedMethods<TAttribute>(this Type t)
			where TAttribute : Attribute
			=> t.GetAnnotatedMethods<TAttribute>(ReflectionUtil.ALL_FLAGS);

		internal static (MethodInfo[], TAttribute[]) GetAnnotatedMethods<TAttribute>(this Type t, BindingFlags flags)
			where TAttribute : Attribute
			=> GetAnnotated(t.GetMethods, flags, info => info.GetCustomAttribute<TAttribute>());

		#endregion

		#region Fields

		internal static (FieldInfo[], TAttribute[]) GetAnnotatedFields<TAttribute>(this Type t)
			where TAttribute : Attribute
			=> t.GetAnnotatedFields<TAttribute>(ReflectionUtil.ALL_FLAGS);

		internal static (FieldInfo[], TAttribute[]) GetAnnotatedFields<TAttribute>(this Type t, BindingFlags flags)
			where TAttribute : Attribute
			=> GetAnnotated(t.GetFields, flags, info => info.GetCustomAttribute<TAttribute>());

		#endregion
	}
}