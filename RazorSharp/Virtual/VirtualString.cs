using System.Runtime.InteropServices;

namespace RazorSharp.Virtual
{

	[StructLayout(LayoutKind.Explicit)]
	public struct VirtualString
	{
		[FieldOffset(0)] private int  m_length;
		[FieldOffset(4)] private char m_firstChar;

	}

}