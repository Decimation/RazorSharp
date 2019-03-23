using System;
using System.Reflection;
using RazorSharp.CoreClr;
using RazorSharp.CoreClr.Structures;

namespace RazorSharp.Memory
{
	public static unsafe class Functions
	{
		static Functions()
		{
			const string FN = "MethodDesc::SetStableEntryPointInterlocked";
			SetEntryPoint = Clr.GetClrFunction<SetEntryPointDelegate>(FN);
		}
		
		/// <summary>
		///     We implement <see cref="SetEntryPointDelegate" /> as a <see cref="Delegate" /> initially because
		///     <see cref="MethodDesc.SetStableEntryPointInterlocked" /> has not been bound yet, and in order to bind it
		///     we have to use this function.
		/// </summary>
		/// <param name="value"><c>this</c> pointer of a <see cref="MethodDesc" /></param>
		/// <param name="pCode">Entry point</param>
		private delegate long SetEntryPointDelegate(MethodDesc* value, ulong pCode);

		private static readonly SetEntryPointDelegate SetEntryPoint;
		
		public static void Swap(MethodInfo dest, MethodInfo src)
		{
			var srcCode = src.MethodHandle.GetFunctionPointer();
			SetStableEntryPoint(dest,srcCode);
		}

		/// <summary>
		///     <remarks>
		///         Equal to <see cref="MethodDesc.SetStableEntryPoint" />, but this is implemented via a <see cref="Delegate" />
		///     </remarks>
		/// </summary>
		public static void SetStableEntryPoint(MethodInfo mi, IntPtr pCode)
		{
			var pMd    = (MethodDesc*) mi.MethodHandle.Value;
			var result = SetEntryPoint(pMd, (ulong) pCode);
			if (!(result > 0)) {
				Global.Log.Warning("Could not set entry point for {Method}", mi.Name);	
			}
			//Conditions.Assert(result >0);
		}
	}
}