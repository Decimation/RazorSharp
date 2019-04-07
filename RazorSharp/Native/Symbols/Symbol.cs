using System;
using System.Runtime.InteropServices;
using RazorSharp.Memory;
using RazorSharp.Memory.Pointers;

namespace RazorSharp.Native.Symbols
{
	/// <summary>
	/// Wraps a <see cref="SymbolInfo"/>
	/// </summary>
	public unsafe class Symbol
	{
		public string Name         { get; }
		public uint   SizeOfStruct { get; }
		public uint   TypeIndex    { get; }
		public uint   Index        { get; }
		public uint   Size         { get; }
		public ulong  ModBase      { get; }
		public uint   Flags        { get; }
		public ulong  Value        { get; }
		public ulong  Address      { get; }
		public uint   Register     { get; }
		public uint   Scope        { get; }
		public uint   Tag          { get; }

		public SymTagEnum TagEnum => (SymTagEnum) Tag;

		public long Offset => (long) (Address - ModBase);

		private readonly byte[] m_symbolStructMemory;


		/// <summary>
		/// Copies the values of <see cref="m_symbolStructMemory"/> into unmanaged memory.
		/// <seealso cref="m_symbolStructMemory"/> contains the wrapped <see cref="SymbolInfo"/> value.
		/// <remarks>
		/// This memory must be freed with <see cref="Mem.Free{T}(Pointer{T})"/>
		/// </remarks>
		/// </summary>
		internal Pointer<SymbolInfo> GetSymbolInfo()
		{
			var alloc = Mem.AllocUnmanaged<byte>(m_symbolStructMemory.Length);
			alloc.WriteAll(m_symbolStructMemory);
			return alloc.Cast<SymbolInfo>();
		}

		internal Symbol(SymbolInfo* pSymInfo)
		{
			Name = NativeHelp.GetString(&pSymInfo->Name, pSymInfo->NameLen);

			SizeOfStruct = pSymInfo->SizeOfStruct;
			TypeIndex    = pSymInfo->TypeIndex;
			Index        = pSymInfo->Index;
			Size         = pSymInfo->Size;
			ModBase      = pSymInfo->ModBase;
			Flags        = pSymInfo->Flags;
			Value        = pSymInfo->Value;
			Address      = pSymInfo->Address;
			Register     = pSymInfo->Register;
			Scope        = pSymInfo->Scope;
			Tag          = pSymInfo->Tag;

			int realSize = GetSymbolInfoSize(pSymInfo);
			m_symbolStructMemory = new byte[realSize];

			Marshal.Copy((IntPtr) pSymInfo, m_symbolStructMemory, 0, realSize);
		}

		private static int GetSymbolInfoSize(SymbolInfo* pSym)
		{
			// SizeOfStruct + (MaxNameLen - 1) * sizeof(TCHAR)
			return (int) (pSym->SizeOfStruct + (pSym->MaxNameLen - 1) * sizeof(byte));
		}

		public override string ToString()
		{
			return String.Format("Name: {0} | Offset: {1:X} | Address: {2:X} | Tag: {3}",
			                     Name,
			                     Offset,
			                     Address,
			                     TagEnum);
		}
	}
}