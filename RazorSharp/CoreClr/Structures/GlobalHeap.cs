using System;
using System.Runtime.CompilerServices;
using RazorSharp.Import;
using RazorSharp.Import.Attributes;
using RazorSharp.Memory.Pointers;

// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable MemberCanBeMadeStatic.Global
// ReSharper disable InconsistentNaming
#pragma warning disable 0649

namespace RazorSharp.CoreClr.Structures
{
	/// <summary>
	/// Provides utilities for working with the GC heap.
	/// </summary>
	[ClrSymNamespace]
	public static class GlobalHeap
	{
		#region Globals

		/// <summary>
		///     <para>Global CLR variable <c>g_pGCHeap</c></para>
		///     <para>Global VM GC</para>
		/// </summary>
		[SymField(SymImportOptions.FullyQualified, SymFieldOptions.LoadFast)]
		private static readonly IntPtr g_pGCHeap;

		/// <summary>
		///     <para>Global CLR variable <c>g_gc_lowest_address</c></para>
		/// </summary>
		[SymField(SymImportOptions.FullyQualified, SymFieldOptions.LoadFast)]
		private static readonly IntPtr g_lowest_address;

		/// <summary>
		///     <para>Global CLR variable <c>g_gc_highest_address</c></para>
		/// </summary>
		[SymField(SymImportOptions.FullyQualified, SymFieldOptions.LoadFast)]
		private static readonly IntPtr g_highest_address;

		#endregion

		#region Accessors

		/// <summary>
		///     Total size of the managed GC heap
		/// </summary>
		public static long Size => Math.Abs(g_highest_address.ToInt64() - g_lowest_address.ToInt64());

		/// <summary>
		///     The lowest address of the global GC heap.
		/// </summary>
		public static Pointer<byte> LowestAddress => g_lowest_address;

		/// <summary>
		///     The highest address of the global GC heap.
		/// </summary>
		public static Pointer<byte> HighestAddress => g_highest_address;

		internal static Pointer<GCHeap> GlobalHeapValue => g_pGCHeap;

		/// <summary>
		/// Returns the number of garbage collections that have occurred.
		/// </summary>
		public static int GCCount => GlobalHeapValue.Reference.GCCount;

		#endregion

		#region Allocate object

		/// <summary>
		/// Allocates a zero-initialized object on the GC heap.
		/// </summary>
		[SymCall(SymImportOptions.FullyQualified)]
		internal static unsafe void* AllocateObject(MethodTable* mt, int fHandleCom = default)
		{
			throw new SymImportException(nameof(AllocateObject));
		}

		private static unsafe T AllocateObjectInternal<T>(Type type, int fHandleCom = default)
		{
			var ptr = AllocateObject(type.GetMethodTable().ToPointer<MethodTable>(), fHandleCom);
			return Unsafe.Read<T>(&ptr);
		}

		/// <summary>
		/// Allocates a zero-initialized object on the GC heap.
		/// </summary>
		internal static object AllocateObject(Type type, int fHandleCom = default)
		{
			return AllocateObjectInternal<object>(type, fHandleCom);
		}

		/// <summary>
		/// Allocates a zero-initialized object on the GC heap.
		/// </summary>
		internal static T AllocateObject<T>(int fHandleCom = default)
		{
			return AllocateObjectInternal<T>(typeof(T), fHandleCom);
		}

		#endregion

		#region Heap pointer

		public static bool IsHeapPointer<T>(T value, bool smallHeapOnly = false) where T : class
		{
			return GlobalHeapValue.Reference.IsHeapPointer(value, smallHeapOnly);
		}

		public static unsafe bool IsHeapPointer(Pointer<byte> obj, bool smallHeapOnly = false)
		{
			return GlobalHeapValue.Reference.IsHeapPointer(obj.ToPointer(), smallHeapOnly);
		}

		#endregion

		static GlobalHeap()
		{
			Symload.Load(typeof(GlobalHeap));
		}
	}
}