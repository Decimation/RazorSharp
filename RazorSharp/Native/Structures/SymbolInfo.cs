using System;
using System.Runtime.InteropServices;

namespace RazorSharp.Native.Structures
{
	#region

	using ULONG = UInt32;
	using ULONG64 = UInt64;
	using CHAR = SByte;

	#endregion
	
	[Native]
	[StructLayout(LayoutKind.Sequential)]
	internal struct SymbolInfo
	{
		internal ULONG   SizeOfStruct;
		internal ULONG   TypeIndex;
		internal ULONG64 Reserved_1;
		internal ULONG64 Reserved_2;
		internal ULONG   Index;
		internal ULONG   Size;
		internal ULONG64 ModBase;
		internal ULONG   Flags;
		internal ULONG64 Value;
		internal ULONG64 Address;
		internal ULONG   Register;
		internal ULONG   Scope;
		internal ULONG   Tag;
		internal ULONG   NameLen;
		internal ULONG   MaxNameLen;
		internal CHAR    Name;
	}
}