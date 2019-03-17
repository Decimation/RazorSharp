#region

#region

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using RazorSharp.CoreClr;
using RazorSharp.Memory.Calling.Signatures.Attributes;
using RazorSharp.Utilities;
using Serilog.Context;

#endregion

// ReSharper disable InconsistentNaming

#endregion

namespace RazorSharp.Memory.Calling.Signatures
{
	// todo: WIP
	// todo: make caching more efficient

	/// <summary>
	///     Contains methods for operating with <see cref="SigcallAttribute" />-annotated functions
	/// </summary>
	public static class SignatureCall
	{
		/// <summary>
		///     <para>Fully bound types</para>
		///     <para>Not including individual methods</para>
		/// </summary>
		private static readonly ISet<Type> BoundTypes = new HashSet<Type>();

		private static readonly SigScanner SigScanner = new SigScanner();


		/// <summary>
		///     When <c>true</c>, only the text (code) segment (which contains executable code)
		///     of the target DLL will be scanned by <see cref="SigScanner" />.
		/// </summary>
		public static bool UseTextSegment { get; set; } = true;

		private static void SelectModule(SigcallAttribute attr)
		{
			if (UseTextSegment && Environment.Is64BitProcess) // todo: Segments 32-bit
				SigScanner.SelectModuleBySegment(attr.Module, Segments.TEXT_SEGMENT);
			else
				SigScanner.SelectModule(attr.Module);
		}

		private static IntPtr GetCorrespondingFunctionPointer(SigcallAttribute attr, MethodInfo methodInfo)
		{
			const int BYTE_TOL = 3;


			return SigScanner.FindPattern(attr.Signature, attr.OffsetGuess, BYTE_TOL);
		}

		

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void ApplySigcallIndependent(MethodInfo methodInfo)
		{
			Conditions.RequiresNotNull(methodInfo, nameof(methodInfo));

			var attr = methodInfo.GetCustomAttribute<SigcallAttribute>();

			if (attr != null) {
				SelectModule(attr);

				var fn = GetCorrespondingFunctionPointer(attr, methodInfo);

				using (SignatureCallLogContext) {
					Global.Log.Verbose("Binding {Name} to {Addr:X}", methodInfo.Name, fn.ToInt64());

					if (fn == IntPtr.Zero) {
						Global.Log.Error("Could not resolve address for func {Name}", methodInfo.Name);
					}
				}

				if (fn != IntPtr.Zero) {
					Global.Log.Debug("Setting entry point for {Name} to {Addr}",
					                 methodInfo.Name, fn.ToInt64().ToString("X"));
					ClrFunctions.SetStableEntryPoint(methodInfo, fn);
				}
			}
		}


		#region Serilog

		// todo: maybe follow this pattern for other classes

		private static IDisposable SignatureCallLogContext =>
			LogContext.PushProperty(Global.CONTEXT_PROP, CONTEXT_PROP_TAG);

		private const string CONTEXT_PROP_TAG = "SignatureCall";

		#endregion

		#region IsBound

		public static bool IsBound<T>()
		{
			return IsBound(typeof(T));
		}

		public static bool IsBound(Type t)
		{
			return BoundTypes.Contains(t);
		}

		#endregion


		#region Bind

		/// <summary>
		///     Binds all functions in type <typeparamref name="T" /> attributed with <see cref="SigcallAttribute" />
		/// </summary>
		/// <typeparam name="T">Type containing unbound <see cref="SigcallAttribute" /> functions</typeparam>
		public static void DynamicBind<T>()
		{
			DynamicBind(typeof(T));
		}


		/// <summary>
		///     Binds all functions in <see cref="Type" /> <paramref name="t" /> attributed with <see cref="SigcallAttribute" />
		/// </summary>
		/// <param name="t">Type containing unbound <see cref="SigcallAttribute" /> functions </param>
		public static void DynamicBind(Type t)
		{
			if (IsBound(t))
				return;

			MethodInfo[] methodInfos = t.GetAllMethods();


			foreach (var mi in methodInfos)
				ApplySigcallIndependent(mi);

			BoundTypes.Add(t);
		}

		/// <summary>
		///     Binds function annotated with <see cref="SigcallAttribute" /> with name <paramref name="name" />
		/// </summary>
		/// <param name="name">Name of the unbound <see cref="SigcallAttribute" /> function </param>
		/// <param name="isGetProperty">Whether the function is a <c>get</c> function of a property </param>
		/// <typeparam name="T">
		///     Type containing unbound <see cref="SigcallAttribute" /> function <paramref name="name" />
		/// </typeparam>
		public static void DynamicBind<T>(string name, bool isGetProperty = false)
		{
			DynamicBind(typeof(T), name, isGetProperty);
		}

		/// <summary>
		///     Binds function annotated with <see cref="SigcallAttribute" /> with name <paramref name="name" />
		/// </summary>
		/// <param name="t">Type containing the unbound <see cref="SigcallAttribute" /> function</param>
		/// <param name="name">Name of the unbound <see cref="SigcallAttribute" /> function </param>
		/// <param name="isGetProperty">Whether the function is a <c>get</c> function of a property </param>
		public static void DynamicBind(Type t, string name, bool isGetProperty = false)
		{
			if (isGetProperty)
				name = Identifiers.NameOfGetPropertyMethod(name);

			var mi = t.GetAnyMethod(name);
			ApplySigcallIndependent(mi);
		}

		#endregion

		
	}
}