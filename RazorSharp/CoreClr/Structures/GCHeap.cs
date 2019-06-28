#region

using System.Runtime;
using RazorSharp.Import;
using RazorSharp.Import.Attributes;
using SimpleSharp.Diagnostics;
using RazorSharp.Memory;

#pragma warning disable 649

// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable MemberCanBeMadeStatic.Global
// ReSharper disable InconsistentNaming

#endregion


namespace RazorSharp.CoreClr.Structures
{
	#region

	using CSUnsafe = System.Runtime.CompilerServices.Unsafe;

	#endregion

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
	[ClrSymNamespace(GC_WORKSTATION)]
	internal unsafe struct GCHeap
	{
		private const string GC_WORKSTATION = "WKS";
		
		/// <summary>
		///     Returns the number of GCs that have occurred.
		///     <remarks>
		///         <para>Source: /src/gc/gcinterface.h: 710</para>
		///     </remarks>
		/// </summary>
		internal int GCCount {
			[SymCall("GetGcCount")]
			get => throw new SymImportException(nameof(GCCount));
		}

		internal bool IsHeapPointer<T>(T value, bool smallHeapOnly = false) where T : class
		{
			return IsHeapPointer(Unsafe.AddressOfHeap(value).ToPointer(), smallHeapOnly);
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
		[SymCall]
		internal bool IsHeapPointer(void* obj, bool smallHeapOnly = false)
		{
			throw new SymImportException(nameof(IsHeapPointer));
		}

		static GCHeap()
		{
			Conditions.Require(!GCSettings.IsServerGC, "GC must be WKS", nameof(GCHeap));

			Symload.Load(typeof(GCHeap));

			// Retrieve the global variables from the data segment of the CLR DLL
		}
	}
}