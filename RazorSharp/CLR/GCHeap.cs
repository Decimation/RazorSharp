#region

using System;
using System.Runtime.InteropServices;
using RazorSharp.Memory;
using RazorSharp.Utilities.Exceptions;

// ReSharper disable MemberCanBeMadeStatic.Global

#endregion

// ReSharper disable InconsistentNaming

namespace RazorSharp.CLR
{


	/// <summary>
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
		///     Address of <see cref="g_pGCHeap" />
		/// </summary>
		private static readonly IntPtr g_pGCHeapAddr;

		/// <summary>
		///     <para>Global CLR variable <c>g_pGCHeap</c></para>
		///     <para>Global VM GC</para>
		/// </summary>
		private static readonly IntPtr g_pGCHeap;


		public static GCHeap* GlobalHeap => (GCHeap*) g_pGCHeap;

		/// <summary>
		///     Returns the number of GCs that have occurred.
		///     <remarks>
		///         <para>Source: /src/gc/gcinterface.h: 710</para>
		///     </remarks>
		/// </summary>
		public int GCCount {
			[CLRSigcall(OffsetGuess = 0x123C60)] get => throw new NotTranspiledException();
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
		/// <param name="obj"></param>
		/// <param name="smallHeapOnly"></param>
		/// <returns></returns>
		[CLRSigcall(OffsetGuess = 0x58E260)]
		public bool IsHeapPointer(void* obj, bool smallHeapOnly = false)
		{
			throw new NotTranspiledException();
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
		/// <param name="obj"></param>
		/// <returns></returns>
		[CLRSigcall(OffsetGuess = 0x129100)]
		public bool IsEphemeral(void* obj)
		{
			throw new NotTranspiledException();
		}

		// 85
		[CLRSigcall(OffsetGuess = 0x3C3C)]
		public bool IsGCInProgress(bool bConsiderGCStart = false)
		{
			throw new NotTranspiledException();
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

			long strMt = (long) Runtime.MethodTableOf<string>();
			IntPtr g_pStringClassAddr =
				Segments.ScanSegment(".data", CLRFunctions.ClrDll, BitConverter.GetBytes(strMt));
			g_pGCHeapAddr = g_pStringClassAddr + IntPtr.Size * 2;
			g_pGCHeap     = Marshal.ReadIntPtr(g_pGCHeapAddr);

			SignatureCall.Transpile<GCHeap>();

//			Console.WriteLine("g_pGCHeap address: {0}", Hex.ToHex(g_pGCHeapAddr));
//			Console.WriteLine("g_pGCHeap: {0}", Hex.ToHex(g_pGCHeap));
		}
	}

}