using System;
using System.Runtime.InteropServices;
using RazorSharp.Memory;
using RazorSharp.Pointers;

namespace RazorSharp.Native.Structures.Symbols
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

		private readonly byte[] m_symbolStructMemory;

		
		/// <summary>
		/// Copies the values of <see cref="m_symbolStructMemory"/> into unmanaged memory.
		/// <seealso cref="m_symbolStructMemory"/> contains the wrapped <see cref="SymbolInfo"/> value.
		/// <remarks>
		/// This memory must be freed with <see cref="Mem.Free{T}(Pointer{T})"/>
		/// </remarks>
		/// </summary>
		public Pointer<SymbolInfo> GetSymbolInfo()
		{
			var alloc = Mem.AllocUnmanaged<byte>(m_symbolStructMemory.Length);
			alloc.WriteAll(m_symbolStructMemory);
			return alloc.Cast<SymbolInfo>();
		}

		internal Symbol(SymbolInfo* pSymInfo)
		{
			Name = Marshal.PtrToStringAuto(new IntPtr(&pSymInfo->Name), (int) pSymInfo->NameLen);

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

			int realSize = CalculateRealStructSize(pSymInfo);
			m_symbolStructMemory = new byte[realSize];

			Marshal.Copy((IntPtr) pSymInfo, m_symbolStructMemory, 0, realSize);
		}

		private static int CalculateRealStructSize(SymbolInfo* pSym)
		{
			// SizeOfStruct + (MaxNameLen - 1) * sizeof(TCHAR)
			return (int) (pSym->SizeOfStruct + (pSym->MaxNameLen - 1) * sizeof(byte));
		}

		public override string ToString()
		{
			return String.Format("Name: {0} | Size: {1} | Type index: {2} | Address: {3:X}", 
			                     Name, 
			                     Size, 
			                     TypeIndex, 
			                     Address);
		}
	}
}