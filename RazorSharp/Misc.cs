#region

using System;
using System.Reflection;
using RazorSharp.CLR.Fixed;
using RazorSharp.Pointers;

#endregion

namespace RazorSharp
{

	/// <summary>
	///     Unsorted methods
	/// </summary>
	internal static class Misc
	{
		/// <summary>
		///     Converts an <see cref="Action" /> to a <see cref="Func{TResult}" /> by returning a dummy <c>default</c>
		///     value of <typeparamref name="T" />.
		/// </summary>
		/// <param name="action"><see cref="Action" /> to convert</param>
		/// <typeparam name="T">Dummy return type</typeparam>
		/// <returns></returns>
		private static Func<T> ProxyAction<T>(Action action)
		{
			return delegate
			{
				action();
				return default;
			};
		}

		internal static void SetChar(this string str, int i, char c)
		{
			ObjectPinner.InvokeWhilePinned(str, delegate
			{
				Pointer<char> lpChar = Unsafe.AddressOfHeap(ref str, OffsetType.StringData).Address;
				lpChar[i] = c;
			});
		}

		internal static void Set(this string str, string s)
		{
			ObjectPinner.InvokeWhilePinned(str, delegate
			{
				Pointer<char> lpChar = Unsafe.AddressOfHeap(ref str, OffsetType.StringData).Address;
				lpChar.WriteAll(s);
			});
		}

		internal static object InvokeGenericMethod(Type t, string name, Type typeArgs, object instance,
			params object[] args)
		{
			MethodInfo method = t.GetMethod(name,
				BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);
			method = method.MakeGenericMethod(typeArgs);
			return method.Invoke(method.IsStatic ? null : instance, args);
		}
	}

}