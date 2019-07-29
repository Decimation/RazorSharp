using RazorSharp.CoreClr.Meta;
using RazorSharp.CoreClr.Metadata;
using RazorSharp.Import.Attributes;
using RazorSharp.Import.Enums;
using RazorSharp.Memory;
using RazorSharp.Memory.Pointers;

// ReSharper disable InconsistentNaming

namespace RazorSharp.CoreClr.Structures
{
	
	[ImportNamespace]
	public static unsafe class GlobalHeap
	{
		[ImportField(IdentifierOptions.FullyQualified, ImportFieldOptions.Fast)]
		private static readonly Pointer<GCHeap> g_pGCHeap;
		
		public static int GCCount => g_pGCHeap.Reference.GCCount;

		public static object AllocateObject(MetaType mt, bool fHandleCom = false)
		{
			var ptr = GCHeap.AllocateObject(mt.Value.ToPointer<MethodTable>(), fHandleCom);
			return Unsafe.Read<object>(&ptr);
		}

		public static T AllocateObject<T>(bool fHandleCom = false)
		{
			return (T) AllocateObject(typeof(T), fHandleCom);
		}

		public static bool IsHeapPointer<T>(T value) => g_pGCHeap.Reference.IsHeapPointer(value);

		public static bool IsHeapPointer(Pointer<byte> value)
		{
			return g_pGCHeap.Reference.IsHeapPointer(value.ToPointer());
		}
	}
}