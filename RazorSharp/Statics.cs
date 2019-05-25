using System;
using System.Reflection;
using RazorSharp.Utilities;
using SimpleSharp.Diagnostics;

namespace RazorSharp
{
	// todo: WIP
	internal static class Statics
	{
		private static bool PropertyEqual(PropertyInfo lhs, PropertyInfo rhs)
		{
			return lhs.Name == rhs.Name
			       && lhs.PropertyType == rhs.PropertyType
			       && lhs.CanRead == rhs.CanRead
			       && lhs.CanWrite == rhs.CanWrite; // todo
		}

		private static bool MethodEqual(MethodInfo lhs, MethodInfo rhs)
		{
			return lhs.Name == rhs.Name
			       && lhs.ReturnType == rhs.ReturnType
			       && lhs.GetParameters() == rhs.GetParameters();
		}

		internal static void Check(Type interfaceType, Type src)
		{
			var interfaceMembers = interfaceType.GetAllMembers();
			var srcMembers       = src.GetAllMembers();

			foreach (var info in interfaceMembers) {
				switch (info.MemberType) {
					case MemberTypes.Method:

						break;
					case MemberTypes.Property:
						break;

					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}
	}
}