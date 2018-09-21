#region

using System;
using System.Diagnostics;
using System.Runtime;
using RazorInvoke;
using RazorSharp.Memory;
using RazorSharp.Pointers;
using RazorSharp.Utilities.Exceptions;
using static RazorSharp.Memory.Mem;

// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable MemberCanBeMadeStatic.Global
// ReSharper disable InconsistentNaming

#endregion



namespace RazorSharp.CLR
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
		public static long Size => g_highest_address.ToInt64() - g_lowest_address.ToInt64();

		public static Pointer<GCHeap> GlobalHeap => (GCHeap*) g_pGCHeap;

		/// <summary>
		///     Returns the number of GCs that have occurred.
		///     <remarks>
		///         <para>Source: /src/gc/gcinterface.h: 710</para>
		///     </remarks>
		/// </summary>
		public int GCCount {
			[CLRSigcall] get => throw new SigcallException();
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
		[CLRSigcall]
		public bool IsHeapPointer(void* obj, bool smallHeapOnly = false)
		{
			throw new SigcallException();
		}


		/// <summary>
		///     Returns whether or not this object resides in an ephemeral generation.
		///     <remarks>
		///         <para>Sources:</para>
		///         <list type="bullet">
		///             <item>
		///                 <description>/src/gc/gcimpl.h: 163</description>
		///             </item>
		///             <item>
		///                 <description>/src/gc/gcinterface.h: 717</description>
		///             </item>
		///         </list>
		///     </remarks>
		/// </summary>
		/// <param name="obj">Pointer to an object in the GC heap</param>
		/// <returns></returns>
		[CLRSigcall]
		public bool IsEphemeral(void* obj)
		{
			throw new SigcallException();
		}

		public bool IsEphemeral<T>(T t) where T : class
		{
			return IsEphemeral(Unsafe.AddressOfHeap(ref t).ToPointer());
		}

		/// <summary>
		///     Returns true if the address of <paramref name="t" /> is in the GC heap.
		/// </summary>
		/// <param name="t">Reference to check</param>
		/// <typeparam name="T">Type of <paramref name="t" /></typeparam>
		/// <returns><c>true</c> if the address of <paramref name="t" /> is in the GC heap; <c>false</c> otherwise</returns>
		public static bool IsInGCHeap<T>(ref T t)
		{
			IntPtr addr = Unsafe.AddressOf(ref t);
			return IsInGCHeap(addr);
		}

		/// <summary>
		///     Returns <c>true</c> if <paramref name="p" /> is in the GC heap.
		/// </summary>
		/// <param name="p">Pointer</param>
		/// <returns><c>true</c> if <paramref name="p" /> is in the GC heap; <c>false</c> otherwise</returns>
		public static bool IsInGCHeap(IntPtr p)
		{
			return IsAddressInRange(g_highest_address, p, g_lowest_address);
		}

		// 85
		[CLRSigcall]
		public bool IsGCInProgress(bool bConsiderGCStart = false)
		{
			throw new SigcallException();
		}


		//constants for the flags parameter to the gc call back

		// #define GC_CALL_INTERIOR            0x1
		// #define GC_CALL_PINNED              0x2
		// #define GC_CALL_CHECK_APP_DOMAIN    0x4


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


//			const long g_pStringClassOffset    = 32;
			const long g_pGCHeapOffset         = 48;
			const long g_lowest_addressOffset  = 40;
			const long g_highest_addressOffset = 408;

			// Retrieve the global variables from the data segment of the CLR DLL

			ImageSectionInfo dataSegment = Segments.GetSegment(".data", CLRFunctions.ClrDll);


			g_pGCHeap         = ReadPointer<byte>(dataSegment.SectionAddress, g_pGCHeapOffset).Address;
			g_lowest_address  = ReadPointer<byte>(dataSegment.SectionAddress, g_lowest_addressOffset).Address;
			g_highest_address = ReadPointer<byte>(dataSegment.SectionAddress, g_highest_addressOffset).Address;

			SignatureCall.DynamicBind<GCHeap>();

//			Console.WriteLine("g_pGCHeap address: {0}", Hex.ToHex(g_pGCHeapAddr));
//			Console.WriteLine("g_pGCHeap: {0}", Hex.ToHex(g_pGCHeap));
		}
	}

}