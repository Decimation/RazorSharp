#region

using System.Runtime.InteropServices;

#endregion

namespace RazorSharp.Native.Structures.Images
{
	[StructLayout(LayoutKind.Sequential)]
	public struct ImageDataDirectory
	{
		/// DWORD->unsigned int
		public readonly uint VirtualAddress;

		/// DWORD->unsigned int
		public readonly uint Size;
	}
}