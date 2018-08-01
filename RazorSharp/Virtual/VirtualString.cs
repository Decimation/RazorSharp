using System.Runtime.InteropServices;

namespace RazorSharp.Virtual
{

	[StructLayout(LayoutKind.Explicit)]
	public unsafe struct VirtualString
	{

		[FieldOffset(0)] private int  m_length;

	}

}