using System;
using System.Runtime.InteropServices;
// ReSharper disable BuiltInTypeReferenceStyle
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace RazorSharp.Native.Structures
{
	using ULONG = UInt32;
	using ULONG64 = UInt64;
	using CHAR = SByte;


	[StructLayout(LayoutKind.Sequential)]
	public struct SymbolInfo
	{
		public readonly ULONG   SizeOfStruct;
		public readonly ULONG   TypeIndex;
		public readonly ULONG64 Reserved_1;
		public readonly ULONG64 Reserved_2;
		public readonly ULONG   Index;
		public readonly ULONG   Size;
		public readonly ULONG64 ModBase;
		public readonly ULONG   Flags;
		public readonly ULONG64 Value;
		public readonly ULONG64 Address;
		public readonly ULONG   Register;
		public readonly ULONG   Scope;
		public readonly ULONG   Tag;
		public readonly ULONG   NameLen;
		public readonly ULONG   MaxNameLen;
		public readonly CHAR    Name;
	} //SYMBOL_INFO, *PSYMBOL_INFO;
}