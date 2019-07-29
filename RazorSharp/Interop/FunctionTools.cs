using System.Collections.Generic;
using System.Reflection;
using RazorSharp.CoreClr;
using RazorSharp.CoreClr.Metadata;
using RazorSharp.Import.Attributes;
using RazorSharp.Import.Enums;
using RazorSharp.Memory;
using RazorSharp.Memory.Pointers;
using RazorSharp.Utilities.Security;

namespace RazorSharp.Interop
{
	[ImportNamespace]
	internal static unsafe class FunctionTools
	{
		static FunctionTools()
		{
			ImportMap = new Dictionary<string, Pointer<byte>>();
		}

		[ImportMap]
		private static readonly Dictionary<string, Pointer<byte>> ImportMap;

		[ImportForwardCall(typeof(MethodDesc), nameof(MethodDesc.Reset), ImportCallOptions.Map)]
		internal static void Restore(MethodInfo mi)
		{
			NativeFunctions.CallVoid((void*) ImportMap[nameof(Restore)], Runtime.ResolveHandle(mi).ToPointer());
		}

		[ImportForwardCall(typeof(MethodDesc), nameof(MethodDesc.SetNativeCodeInterlocked), ImportCallOptions.Map)]
		internal static bool SetEntryPoint(MethodInfo mi, Pointer<byte> ptr)
		{
			if (!Mem.Is64Bit) {
				throw Guard.Require64BitFail(nameof(SetEntryPoint));
			}

			Restore(mi);

			return NativeFunctions.Call<bool>((void*) ImportMap[nameof(SetEntryPoint)],
			                                  mi.MethodHandle.Value.ToPointer(), ptr.ToPointer());
		}
	}
}