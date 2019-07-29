using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

namespace RazorSharp.CoreClr.Metadata.ExecutionEngine
{
	[StructLayout(LayoutKind.Explicit)]
	internal struct LayoutEEClass
	{
		// Note: This offset should be 72 or sizeof(EEClass)
		// 		 but I'm keeping it at 0 to minimize size usage,
		//		 so I'll just offset the pointer by 72 bytes
		[FieldOffset(0)]
		internal EEClassLayoutInfo m_LayoutInfo;
	}
}