#region

using System.Runtime.InteropServices;

#endregion

namespace RazorSharp.Native.Structures.Images
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct ImageNtHeaders64
	{
		public readonly uint                  Signature;
		public          ImageFileHeader       FileHeader;
		public          ImageOptionalHeader64 OptionalHeader;
	}
}