#region

using System.Runtime.InteropServices;

// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable MemberCanBePrivate.Global

#endregion

namespace RazorSharp.Native.Images
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	internal struct ImageNtHeaders64
	{
		public uint                  Signature;
		public ImageFileHeader       FileHeader;
		public ImageOptionalHeader64 OptionalHeader;
	}
}