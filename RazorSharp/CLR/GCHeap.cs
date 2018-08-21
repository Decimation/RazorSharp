#region

using System;
using System.Runtime.InteropServices;

#endregion

// ReSharper disable InconsistentNaming

namespace RazorSharp.CLR
{

	/// <summary>
	///     <list type="bullet">
	///         <item>
	///             <description>/src/vm/gcheaputilities.cpp</description>
	///         </item>
	///         <item>
	///             <description>/src/gc/gcimpl.h</description>
	///         </item>
	///     </list>
	/// </summary>
	public static unsafe class GCHeap
	{
		private static readonly IntPtr g_pGCHeapAddr = new IntPtr(0x7FFAEF324030);
		private static readonly IntPtr g_pGCHeap;

		public static int GCCount => (int) CLRFunctions.GCFunctions.GetGCCountInternal(g_pGCHeap.ToPointer());

		public static bool IsHeapPointer<T>(T t, bool smallHeapOnly = false) where T : class
		{
			return IsHeapPointer(Unsafe.AddressOfHeap(ref t).ToPointer(), smallHeapOnly);
		}

		// 164
		public static bool IsHeapPointer(void* obj, bool smallHeapOnly = false)
		{
			return CLRFunctions.GCFunctions.IsHeapPointer(g_pGCHeap.ToPointer(), obj, smallHeapOnly);
		}

		// 163
		public static bool IsEphemeral(void* obj)
		{
			return CLRFunctions.GCFunctions.IsEphemeral(g_pGCHeap.ToPointer(), obj);
		}

		// 85
		public static bool IsGCInProgress(bool bConsiderGCStart = false)
		{
			return CLRFunctions.GCFunctions.IsGCInProgress(bConsiderGCStart);
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

//			long   strMt              = (long) Runtime.MethodTableOf<string>();
//			var    g_pStringClassAddr = Segments.ScanSegment(".data", "clr.dll", BitConverter.GetBytes(strMt));
//			IntPtr g_pGCHeapAddr      = g_pStringClassAddr + IntPtr.Size * 2;

			g_pGCHeap = Marshal.ReadIntPtr(g_pGCHeapAddr);

//			Console.WriteLine("g_pGCHeap address: {0}", Hex.ToHex(g_pGCHeapAddr));
//			Console.WriteLine("g_pGCHeap: {0}", Hex.ToHex(g_pGCHeap));
		}
	}

}