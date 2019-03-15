#region

using System;
using System.Diagnostics;
using System.Runtime;
using RazorSharp.Memory;
using RazorSharp.Memory.Calling.Symbols;
using RazorSharp.Memory.Calling.Symbols.Attributes;
using RazorSharp.Native;
using RazorSharp.Pointers;
using RazorSharp.Utilities;
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
			[ClrSymcall(Symbol = "WKS::GCHeap::GetGcCount", FullyQualified = true)]
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
		[ClrSymcall(Symbol = "WKS::GCHeap::IsHeapPointer", FullyQualified = true)]
		public bool IsHeapPointer(void* obj, bool smallHeapOnly = false)
		{
			throw new SigcallException();
		}

		// 85
		[ClrSymcall(Symbol = "WKS::GCHeap::IsGCInProgress", FullyQualified = true)]
		public bool IsGCInProgress(bool bConsiderGCStart = false)
		{
			throw new SigcallException();
		}
		
		static GCHeap()
		{
#if !UNIT_TEST
			Conditions.RequiresWorkstationGC();
#endif
			
			Symcall.BindQuick(typeof(GCHeap));

			// Retrieve the global variables from the data segment of the CLR DLL

			using (var sym = new Symbolism(Symbolism.CLR_PDB)) {
				g_pGCHeap = sym.GetSymAddress(nameof(g_pGCHeap), Clr.CLR_DLL).Address;
				g_lowest_address = sym.GetSymAddress(nameof(g_lowest_address), Clr.CLR_DLL).Address;
				g_highest_address = sym.GetSymAddress(nameof(g_highest_address), Clr.CLR_DLL).Address;
			}
		}
	}
}