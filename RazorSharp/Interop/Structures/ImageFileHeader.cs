#region

using System.Runtime.InteropServices;
using RazorSharp.Interop.Utilities;

#endregion

namespace RazorSharp.Interop.Structures
{
	[Native]
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct ImageFileHeader
	{
		public ushort Machine { get; }

		public ushort NumberOfSections { get; }

		public uint TimeDateStamp { get; }

		public uint PointerToSymbolTable { get; }

		public uint NumberOfSymbols { get; }

		public ushort SizeOfOptionalHeader { get; }

		public ushort Characteristics { get; }
	}
}