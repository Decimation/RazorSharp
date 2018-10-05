using System.Runtime.InteropServices;

namespace RazorSharp.Native.Structures
{

	[StructLayout(LayoutKind.Sequential)]
	public struct FLOAT128
	{
		long LowPart;
		long HighPart;
	}

}