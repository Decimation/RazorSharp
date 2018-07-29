using System.Runtime.InteropServices;

namespace RazorSharp.Runtime.CLRTypes
{
	//fixme
	[StructLayout(LayoutKind.Explicit)]
	// ReSharper disable once InconsistentNaming
	public struct LayoutEEClass
	{
		[FieldOffset(0)]
		public EEClassLayoutInfo m_LayoutInfo;
	}

}