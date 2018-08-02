using System.Runtime.InteropServices;

namespace RazorSharp.Virtual
{

	[StructLayout(LayoutKind.Explicit)]
	public unsafe struct VirtualString
	{

		private char[] m_value;


	}

}