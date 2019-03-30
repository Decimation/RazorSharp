using System;
using System.Runtime.InteropServices;

// ReSharper disable BuiltInTypeReferenceStyle
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace RazorSharp.Native.Structures.Symbols
{
	using ULONG = UInt32;
	using ULONG64 = UInt64;
	using CHAR = SByte;


	[StructLayout(LayoutKind.Sequential)]
	public struct SymbolInfo
	{
		public ULONG   SizeOfStruct;
		public ULONG   TypeIndex;
		public ULONG64 Reserved_1;
		public ULONG64 Reserved_2;
		public ULONG   Index;
		public ULONG   Size;
		public ULONG64 ModBase;
		public ULONG   Flags;
		public ULONG64 Value;
		public ULONG64 Address;
		public ULONG   Register;
		public ULONG   Scope;
		public ULONG   Tag;
		public ULONG   NameLen;
		public ULONG   MaxNameLen;
		public CHAR    Name;
	} //SYMBOL_INFO, *PSYMBOL_INFO;
}