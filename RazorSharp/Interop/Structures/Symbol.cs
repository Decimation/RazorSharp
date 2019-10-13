using System;
using System.Runtime.InteropServices;
using RazorSharp.Interop.Enums;
using RazorSharp.Interop.Utilities;
using RazorSharp.Memory;

namespace RazorSharp.Interop.Structures
{
	/// <summary>
	///     Wraps a <see cref="SymbolInfo" />
	/// </summary>
	public unsafe class Symbol
	{
		internal Symbol(SymbolInfo* pSymInfo, string name)
		{
			Name = name;

			SizeOfStruct = pSymInfo->SizeOfStruct;
			TypeIndex    = pSymInfo->TypeIndex;
			Index        = pSymInfo->Index;
			Size         = pSymInfo->Size;
			ModBase      = pSymInfo->ModBase;
			Flags        = (SymbolFlag) pSymInfo->Flags;
			Value        = pSymInfo->Value;
			Address      = pSymInfo->Address;
			Register     = pSymInfo->Register;
			Scope        = pSymInfo->Scope;
			Tag          = (SymbolTag) pSymInfo->Tag;
		}

		internal Symbol(IntPtr pSym) : this((SymbolInfo*) pSym, Native.DebugHelp.GetSymbolName(pSym)) { }


		public string Name { get; }

		public uint SizeOfStruct { get; }

		public uint TypeIndex { get; }

		public uint Index { get; }

		public uint Size { get; }

		public ulong ModBase { get; }

		public ulong Value { get; }

		public ulong Address { get; }

		public uint Register { get; }

		public uint Scope { get; }

		public SymbolTag Tag { get; }

		public SymbolFlag Flags { get; }

		public long Offset => (long) (Address - ModBase);


		private static int GetSymbolInfoSize(SymbolInfo* pSym)
		{
			// SizeOfStruct + (MaxNameLen - 1) * sizeof(TCHAR)
			return (int) (pSym->SizeOfStruct + (pSym->MaxNameLen - 1) * sizeof(byte));
		}

		public override string ToString()
		{
			return String.Format("Name: {0} | Offset: {1:X} | Address: {2:X} | Tag: {3} | Flags: {4}", Name, Offset,
			                     Address, Tag, Flags);
		}

		internal const uint MAX_SYM_NAME = 2000;

		internal static readonly int StructureSize = Marshal.SizeOf<SymbolInfo>();
	}
}