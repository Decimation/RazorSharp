#region

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using RazorSharp.CoreClr;
using RazorSharp.CoreClr.Structures;

#endregion

namespace RazorSharp.Memory
{
	public static unsafe class Functions
	{
		static Functions()
		{
			const string SET_ENTRY_POINT = "MethodDesc::SetStableEntryPointInterlocked";
			SetEntryPoint = Clr.GetClrFunction<SetEntryPointDelegate>(SET_ENTRY_POINT);

			Global.Log.Debug("Functions init done");

			/*const string GET_DELEGATE = "GetDelegateForFunctionPointerInternal";
			GetDelegate = (GetDelegateDelegate) typeof(Marshal)
			                                   .GetAnyMethod(GET_DELEGATE)
			                                   .CreateDelegate(typeof(GetDelegateDelegate));*/
		}


		#region Set entry point

		/// <summary>
		///     We implement <see cref="SetEntryPointDelegate" /> as a <see cref="Delegate" /> initially because
		///     <see cref="MethodDesc.SetStableEntryPointInterlocked" /> has not been bound yet, and in order to bind
		///     it we have to use this function.
		/// </summary>
		/// <param name="value"><c>this</c> pointer of a <see cref="MethodDesc" /></param>
		/// <param name="pCode">Entry point</param>
		private delegate long SetEntryPointDelegate(MethodDesc* value, ulong pCode);

		private static readonly SetEntryPointDelegate SetEntryPoint;


		/// <summary>
		///     <remarks>
		///         Equal to <see cref="MethodDesc.SetStableEntryPoint" />, but this is implemented via a <see cref="Delegate" />
		///     </remarks>
		/// </summary>
		public static void SetStableEntryPoint(MethodInfo mi, IntPtr pCode)
		{
			var  pMd    = (MethodDesc*) mi.MethodHandle.Value;
			long result = SetEntryPoint(pMd, (ulong) pCode);
			if (!(result > 0)) {
				Global.Log.Warning("Could not set entry point for {Method}", mi.Name);
			}

			//Conditions.Assert(result >0);
		}

		#endregion

		#region Get delegate

		public static TDelegate GetDelegateForFunctionPointer<TDelegate>(IntPtr ptr) where TDelegate : Delegate
		{
			return (TDelegate) GetDelegateForFunctionPointer(ptr, typeof(TDelegate));
		}

		public static Delegate GetDelegateForFunctionPointer(IntPtr ptr, Type t)
		{
//			Conditions.RequiresNotNull(GetDelegate, nameof(GetDelegate));
//			return GetDelegate(ptr, t);

			return Marshal.GetDelegateForFunctionPointer(ptr, t);
		}

		public static void Swap(MethodInfo dest, MethodInfo src)
		{
			var srcCode = src.MethodHandle.GetFunctionPointer();
			SetStableEntryPoint(dest, srcCode);
		}

		#endregion
	}
}