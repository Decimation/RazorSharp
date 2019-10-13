using RazorSharp.Core;
using RazorSharp.CoreClr.Meta;
using RazorSharp.CoreClr.Metadata;
using RazorSharp.Import;
using RazorSharp.Import.Attributes;
using RazorSharp.Import.Enums;
using RazorSharp.Interop;
using RazorSharp.Memory;
using RazorSharp.Memory.Pointers;

// ReSharper disable InconsistentNaming

namespace RazorSharp.CoreClr
{
	[ImportNamespace(WKS_NAMESPACE)]
	public static unsafe class GCHeap
	{
		private const string WKS_NAMESPACE = "WKS";

		static GCHeap()
		{
			ImportManager.Value.Load(typeof(GCHeap), Clr.Value.Imports);
		}

		[ImportField(IdentifierOptions.FullyQualified, ImportFieldOptions.Fast)]
		private static readonly Pointer<byte> g_pGCHeap;

		[ImportMapDesignation]
		private static readonly ImportMap Imports = new ImportMap();

		public static int GCCount {
			[ImportCall("GetGcCount", ImportCallOptions.Map)]
			get => Functions.Native.Call<int>((void*) Imports[nameof(GCCount)], g_pGCHeap.ToPointer());
		}

		public static object AllocateObject(MetaType mt, bool fHandleCom = false)
		{
			void* ptr = AllocateObject(mt.Value.ToPointer<MethodTable>(), fHandleCom);
			return Unsafe.Read<object>(&ptr);
		}

		public static T AllocateObject<T>(bool fHandleCom = false)
		{
			return (T) AllocateObject(typeof(T), fHandleCom);
		}


		[ImportCall(IdentifierOptions.FullyQualified, ImportCallOptions.Map)]
		public static void* AllocateObject(Pointer<MethodTable> mt, bool fHandleCom = false)
		{
			return Functions.Native.CallReturnPointer((void*) Imports[nameof(AllocateObject)],
			                                          mt.ToPointer<MethodTable>(), fHandleCom);
		}

		[ImportCall(ImportCallOptions.Map)]
		public static bool IsHeapPointer(Pointer<byte> p, bool smallHeapOnly = false)
		{
			return Functions.Native.Call<bool, bool>((void*) Imports[nameof(IsHeapPointer)],
			                                         g_pGCHeap.ToPointer(), p.ToPointer(), smallHeapOnly);
		}

		public static bool IsHeapPointer<T>(T value, bool smallHeapOnly = false)
		{
			return Unsafe.TryGetAddressOfHeap(value, out Pointer<byte> ptr) &&
			       IsHeapPointer(ptr.ToPointer(), smallHeapOnly);
		}
	}
}