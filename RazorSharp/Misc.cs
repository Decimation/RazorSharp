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
		internal static void SetChar(this string str, int i, char c)
		{
			ObjectPinner.InvokeWhilePinned(str, delegate
			{
				Pointer<char> lpChar = Unsafe.AddressOfHeap(ref str, OffsetType.StringData);
				lpChar[i] = c;
			});
		}

		internal static void Set(this string str, string s)
		{
			ObjectPinner.InvokeWhilePinned(str, delegate
			{
				Pointer<char> lpChar = Unsafe.AddressOfHeap(ref str, OffsetType.StringData);
				for (int i = 0; i < str.Length; i++) {
					lpChar[i] = s[i];
				}
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