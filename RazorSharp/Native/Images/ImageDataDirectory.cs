#region

using System.Runtime.InteropServices;

#endregion

// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable MemberCanBePrivate.Global

namespace RazorSharp.Native.Images
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct ImageDataDirectory
	{
		public uint VirtualAddress;
		public uint Size;
	}
}