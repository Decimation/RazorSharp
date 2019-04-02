#region

using System.Runtime.InteropServices;

// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable MemberCanBePrivate.Global

#endregion

namespace RazorSharp.Native.Structures.Images
{
	[StructLayout(LayoutKind.Sequential)]
	public struct ImageDataDirectory
	{
		/// DWORD->unsigned int
		public uint VirtualAddress;

		/// DWORD->unsigned int
		public uint Size;
	}
}