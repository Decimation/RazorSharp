using System.Collections.Generic;
using RazorSharp.CoreClr.Metadata;
using RazorSharp.Import.Attributes;
using RazorSharp.Import.Enums;
using RazorSharp.Interop;
using RazorSharp.Memory;
using RazorSharp.Memory.Pointers;

// ReSharper disable InconsistentNaming

namespace RazorSharp.CoreClr.Structures
{
	

	[ImportNamespace(WKS_NAMESPACE)]
	internal unsafe struct GCHeap
	{
		private const string WKS_NAMESPACE = "WKS";

		static GCHeap()
		{
			ImportMap = new Dictionary<string, Pointer<byte>>();
		}
		

		[ImportMap]
		private static readonly Dictionary<string, Pointer<byte>> ImportMap;
		
		internal int GCCount {
			[ImportCall("GetGcCount", ImportCallOptions.Map)]
			get {
				fixed (GCHeap* value = &this) {
					return Functions.Native.Call<int>((void*) ImportMap[nameof(GCCount)], value);
				}
			}
		}

		[ImportCall(IdentifierOptions.FullyQualified, ImportCallOptions.Map)]
		internal static void* AllocateObject(MethodTable* mt, bool fHandleCom = false)
		{
			return Functions.Native.CallReturnPointer((void*) ImportMap[nameof(AllocateObject)], mt, fHandleCom);
		}

		[ImportCall(ImportCallOptions.Map)]
		internal bool IsHeapPointer(void* p, bool smallHeapOnly = false)
		{
			fixed (GCHeap* value = &this) {
				return Functions.Native.Call<bool, bool>((void*) ImportMap[nameof(IsHeapPointer)], value, 
				                               p, smallHeapOnly);
			}
		}

		internal bool IsHeapPointer<T>(T value, bool smallHeapOnly = false)
		{
			if (Unsafe.TryGetAddressOfHeap(value, out var ptr)) {
				return IsHeapPointer(ptr.ToPointer(), smallHeapOnly);
			}

			return false;
		}
	}
}