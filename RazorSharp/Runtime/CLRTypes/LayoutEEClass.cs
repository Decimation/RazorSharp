using System.Runtime.InteropServices;
// ReSharper disable InconsistentNaming

namespace RazorSharp.Runtime.CLRTypes
{

	[StructLayout(LayoutKind.Explicit)]
	internal struct LayoutEEClass
	{
		// Note: This offset should be 72 or sizeof(EEClass)
		// 		 but I'm keeping it at 0 to minimize size usage
		[FieldOffset(0)]
		public EEClassLayoutInfo m_LayoutInfo;
	}

}