#region

using System;
using System.Diagnostics;
using System.Reflection;
using RazorCommon;
using RazorSharp.CLR;
using RazorSharp.CLR.Structures;
using RazorSharp.Pointers;

#endregion

namespace RazorSharp.Memory
{


	/// <summary>
	///     Indicates that the attributed function is exposed via signature scanning (using <see cref="SigScanner" />
	///     internally).
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class SigcallAttribute : Attribute
	{
		/// <summary>
		///     Module containing <see cref="Signature" />
		/// </summary>
		public string Module { get; }

		/// <summary>
		///     Unique byte-sequence-string signature of the function
		/// </summary>
		public string Signature { get; }


		public SigcallAttribute(string module, string signature)
		{
			Module    = module;
			Signature = signature;
		}
	}

	/*public class SigcallRangeAttribute : Attribute
	{
		/// <summary>
		///     Module containing <see cref="Signature" />
		/// </summary>
		public string Module { get; }

		/// <summary>
		///     Unique byte sequence string signature of the function
		/// </summary>
		public byte[] Signature { get; }

		public SigcallRangeAttribute(string module, params byte[] signature)
		{
			Module    = module;
			Signature = signature;
		}
	}*/

	/// <summary>
	///     Contains methods for operating with <see cref="SigcallAttribute" />-annotated functions
	/// </summary>
	public static class SignatureCall
	{
		private static readonly SigScanner SigScanner = new SigScanner();

		/// <summary>
		///     Binds all functions in type <typeparamref name="T" /> attributed with <see cref="SigcallAttribute" />
		/// </summary>
		/// <typeparam name="T">Type containing unbound <see cref="SigcallAttribute" /> functions</typeparam>
		public static void Transpile<T>()
		{
			Transpile(typeof(T));
		}

		/// <summary>
		///     Binds all functions in <see cref="Type" /> <paramref name="t" /> attributed with <see cref="SigcallAttribute" />
		/// </summary>
		/// <param name="t">Type containing unbound <see cref="SigcallAttribute" /> functions </param>
		public static void Transpile(Type t)
		{
			Pointer<MethodDesc>[] mds = Runtime.GetMethodDescs(t);

			foreach (Pointer<MethodDesc> md in mds) {
				SigcallAttribute attr = md.Reference.MethodInfo.GetCustomAttribute<SigcallAttribute>();
				if (attr != null) {
					SigScanner.SelectModule(attr.Module);
					IntPtr fn = SigScanner.FindPattern(attr.Signature);

					md.Reference.SetFunctionPointer(fn);
//					Logger.Log("Bound function {0} to {1}", md.Reference.Name, Hex.ToHex(fn));
				}
			}
		}

		/// <summary>
		///     Binds function annotated with <see cref="SigcallAttribute" /> with name <paramref name="name" />
		/// </summary>
		/// <param name="name">Name of the unbound <see cref="SigcallAttribute" /> function </param>
		/// <param name="isGetProperty">Whether the function is a <c>get</c> function of a property </param>
		/// <typeparam name="T">
		///     Type containing unbound <see cref="SigcallAttribute" /> function <paramref name="name" />
		/// </typeparam>
		public static void Transpile<T>(string name, bool isGetProperty = false)
		{
			Transpile(typeof(T), name, isGetProperty);
		}

		/// <summary>
		///     Binds function annotated with <see cref="SigcallAttribute" /> with name <paramref name="name" />
		/// </summary>
		/// <param name="t">Type containing the unbound <see cref="SigcallAttribute" /> function</param>
		/// <param name="name">Name of the unbound <see cref="SigcallAttribute" /> function </param>
		/// <param name="isGetProperty">Whether the function is a <c>get</c> function of a property </param>
		public static unsafe void Transpile(Type t, string name, bool isGetProperty = false)
		{
			if (isGetProperty) {
				name = "get_" + name;
			}

			var      md   = Runtime.GetMethodDesc(t, name);
			SigcallAttribute attr = md.Reference.MethodInfo.GetCustomAttribute<SigcallAttribute>();
			SigScanner.SelectModule(attr.Module);
			IntPtr fn = SigScanner.FindPattern(attr.Signature);
			md.Reference.SetFunctionPointer(fn);
		}
	}

}