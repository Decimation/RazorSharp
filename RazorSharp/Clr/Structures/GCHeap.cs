#region

using System;
using System.Diagnostics;
using System.Runtime;
using RazorSharp.Memory;
using RazorSharp.Pointers;
using RazorSharp.Utilities.Exceptions;

// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable MemberCanBeMadeStatic.Global
// ReSharper disable InconsistentNaming

#endregion


namespace RazorSharp.Clr.Structures
{
	/// <summary>
	///     <para>Represents the entire GC heap. This includes Gen 0, 1, 2, LOH, and other segments.</para>
	///     <para>Corresponding files:</para>
	///     <list type="bullet">
	///         <item>
	///             <description>/src/vm/gcheaputilities.cpp</description>
	///         </item>
	///         <item>
	///             <description>/src/gc/gcimpl.h</description>
	///         </item>
	///         <item>
	///             <description>/src/gc/gcinterface.h</description>
	///         </item>
	///     </list>
	/// </summary>
	public unsafe struct GCHeap
	{
		/// <summary>
		///     <para>Global CLR variable <c>g_pGCHeap</c></para>
		///     <para>Global VM GC</para>
		/// </summary>
		private static readonly IntPtr g_pGCHeap;

		/// <summary>
		///     <para>Global CLR variable <c>g_gc_lowest_address</c></para>
		/// </summary>
		private static readonly IntPtr g_lowest_address;

		/// <summary>
		///     <para>Global CLR variable <c>g_gc_highest_address</c></para>
		/// </summary>
		private static readonly IntPtr g_highest_address;

		/// <summary>
		///     The lowest address of the global GC heap.
		/// </summary>
		public static IntPtr LowestAddress => g_lowest_address;

		/// <summary>
		///     The highest address of the global GC heap.
		/// </summary>
		public static IntPtr HighestAddress => g_highest_address;

		/// <summary>
		///     Total size of the managed GC heap
		/// </summary>
		public static long Size => Math.Abs(g_highest_address.ToInt64() - g_lowest_address.ToInt64());

		public static Pointer<GCHeap> GlobalHeap => (GCHeap*) g_pGCHeap;

		/// <summary>
		///     Returns the number of GCs that have occurred.
		///     <remarks>
		///         <para>Source: /src/gc/gcinterface.h: 710</para>
		///     </remarks>
		/// </summary>
		public int GCCount {
			[ClrSigcall]
			get => throw new SigcallException();
		}

		public bool IsHeapPointer<T>(T t, bool smallHeapOnly = false) where T : class
		{
			return IsHeapPointer(Unsafe.AddressOfHeap(ref t).ToPointer(), smallHeapOnly);
		}

		/// <summary>
		///     Returns true if this pointer points into a GC heap, false otherwise.
		///     <remarks>
		///         <para>Sources:</para>
		///         <list type="bullet">
		///             <item>
		///                 <description>/src/gc/gcimpl.h: 164</description>
		///             </item>
		///             <item>
		///                 <description>/src/gc/gcinterface.h: 700</description>
		///             </item>
		///         </list>
		///     </remarks>
		/// </summary>
		/// <param name="obj">Pointer to an object in the GC heap</param>
		/// <param name="smallHeapOnly">Whether to include small GC heaps only</param>
		/// <returns><c>true</c> if <paramref name="obj" /> is a heap pointer; <c>false</c> otherwise</returns>
		[ClrSigcall]
		public bool IsHeapPointer(void* obj, bool smallHeapOnly = false)
		{
			throw new SigcallException();
		}

		

		// 85
		[ClrSigcall]
		public bool IsGCInProgress(bool bConsiderGCStart = false)
		{
			throw new SigcallException();
		}

		static GCHeap()
		{
			// 	   .data:0000000180944020                               ; class MethodTable * g_pStringClass
			// >>> .data:0000000180944020 00 00 00 00 00 00 00 00       ?g_pStringClass@@3PEAVMethodTable@@EA dq 0
			// 	   .data:0000000180944020                                                                       ; DATA XREF: AllocateStringFastMP_InlineGetThread↑r
			// 	   .data:0000000180944020                                                                       ; AllocateStringFastMP+14↑r ...
			// 	   .data:0000000180944028 00 00 00 00 00 00 00 00       g_lowest_address dq 0                   ; DATA XREF: JIT_CheckedWriteBarrier↑r
			// 	   .data:0000000180944028                                                                       ; JIT_ByRefWriteBarrier+6↑r ...
			// 	   .data:0000000180944030                               ; class GCHeap * g_pGCHeap
			// >>> .data:0000000180944030 00 00 00 00 00 00 00 00       ?g_pGCHeap@@3PEAVGCHeap@@EA dq 0        ; DATA XREF: GCHeap::IsGCInProgress(int)+F↑r

			/**
			 * Circumvent ASLR
			 */

#if !UNIT_TEST
			Trace.Assert(!GCSettings.IsServerGC, "Server GC");
#endif

			// Retrieve the global variables from the data segment of the CLR DLL

			var dataSegment = Segments.GetSegment(".data", ClrFunctions.CLR_DLL);


			g_pGCHeap =
				Mem.ReadPointer<byte>(dataSegment.SectionAddress, Offsets.GLOBAL_GCHEAP_OFFSET).Address;

			g_lowest_address =
				Mem.ReadPointer<byte>(dataSegment.SectionAddress, Offsets.GLOBAL_LOWEST_ADDRESS_OFFSET).Address;

			g_highest_address =
				Mem.ReadPointer<byte>(dataSegment.SectionAddress, Offsets.GLOBAL_HIGHEST_ADDRESS_OFFSET).Address;

			SignatureCall.DynamicBind<GCHeap>();
		}
	}
}