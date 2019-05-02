#region

#region

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using RazorCommon.Diagnostics;
using RazorSharp.Memory.Extern.Sigscan.Attributes;
using RazorSharp.Utilities;

#endregion

// ReSharper disable InconsistentNaming

#endregion

namespace RazorSharp.Memory.Extern.Sigscan
{
	/// <summary>
	///     Contains methods for operating with <see cref="SigcallAttribute" />-annotated functions
	/// </summary>
	[Obsolete]
	public static class SignatureCall
	{
		/// <summary>
		///     <para>Fully bound types</para>
		///     <para>Not including individual methods</para>
		/// </summary>
		private static readonly ISet<Type> BoundTypes = new HashSet<Type>();

		private static readonly MemScanner Scanner = new MemScanner();


		private static IntPtr GetCorrespondingFunctionPointer(SigcallAttribute attr)
		{
			return Scanner.FindPattern(attr.Signature).Address;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void ApplySigcall(MethodInfo methodInfo)
		{
			Conditions.NotNull(methodInfo, nameof(methodInfo));

			var attr = methodInfo.GetCustomAttribute<SigcallAttribute>();

			if (attr != null) {
				Scanner.SelectRegion(Region.FromModule(Modules.GetModule(attr.Module)));

				var fn = GetCorrespondingFunctionPointer(attr);

				if (fn == IntPtr.Zero) {
					Global.Log.Error("Could not resolve address for func {Name}", methodInfo.Name);
				}

				if (fn != IntPtr.Zero) {
					Global.Log.Debug("Setting entry point for {Name} to {Addr}",
					                 methodInfo.Name, fn.ToInt64().ToString("X"));
					Functions.SetStableEntryPoint(methodInfo, fn);
				}
			}
		}


		#region IsBound

		private static bool IsBound(Type t)
		{
			return BoundTypes.Contains(t);
		}

		#endregion


		#region Bind

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
				ApplySigcall(mi);

			BoundTypes.Add(t);
		}

		#endregion
	}
}