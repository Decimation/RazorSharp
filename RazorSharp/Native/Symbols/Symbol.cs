#region

using System;
using System.Runtime.InteropServices;
using RazorSharp.Memory;
using RazorSharp.Memory.Pointers;

#endregion

namespace RazorSharp.Native.Symbols
{
	/// <summary>
	///     Wraps a <see cref="SymbolInfo" />
	/// </summary>
	public unsafe class Symbol
	{
		/// <summary>
		///     Memory of the original <see cref="SymbolInfo" />
		/// </summary>
		private readonly byte[] m_symStructMem;

		internal Symbol(SymbolInfo* pSymInfo, string name)
		{
			Name = name;

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
			m_symStructMem = new byte[realSize];

			Marshal.Copy((IntPtr) pSymInfo, m_symStructMem, 0, realSize);
		}

		internal Symbol(SymbolInfo* pSymInfo)
			: this(pSymInfo, NativeHelp.GetString(&pSymInfo->Name, pSymInfo->NameLen)) { }

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


		/// <summary>
		///     Copies the values of <see cref="m_symStructMem" /> into unmanaged memory.
		///     <seealso cref="m_symStructMem" /> contains the wrapped <see cref="SymbolInfo" /> value.
		///     <remarks>
		///         This memory must be freed with <see cref="Mem.Free{T}(Pointer{T})" />
		///     </remarks>
		/// </summary>
		internal Pointer<SymbolInfo> GetSymbolInfo()
		{
			Pointer<byte> alloc = Mem.Alloc<byte>(m_symStructMem.Length);
			alloc.WriteAll(m_symStructMem);
			return alloc.Cast<SymbolInfo>();
		}

		private static int GetSymbolInfoSize(SymbolInfo* pSym)
		{
			// SizeOfStruct + (MaxNameLen - 1) * sizeof(TCHAR)
			return (int) (pSym->SizeOfStruct + (pSym->MaxNameLen - 1) * sizeof(byte));
		}

		public override string ToString()
		{
			return String.Format("Name: {0} | Offset: {1:X} | Address: {2:X} | Tag: {3} | Size: {4}",
			                     Name, Offset, Address, TagEnum, Size);
		}
	}
}