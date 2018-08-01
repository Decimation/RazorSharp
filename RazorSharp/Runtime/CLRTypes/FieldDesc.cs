using System;
using System.Runtime.InteropServices;
using RazorCommon;

namespace RazorSharp.Runtime.CLRTypes
{

	using unsigned = UInt32;
	using Memory = Memory.Memory;


	/// <summary>
	/// Source: https://github.com/dotnet/coreclr/blob/59714b683f40fac869050ca08acc5503e84dc776/src/vm/field.cpp
	/// Source 2: https://github.com/dotnet/coreclr/blob/59714b683f40fac869050ca08acc5503e84dc776/src/vm/field.h#L43
	///
	/// Internal representation: FieldHandle.Value
	/// </summary>
	[StructLayout(LayoutKind.Explicit)]
	public unsafe struct FieldDesc
	{
		// This actually only seems to be a byte
		[FieldOffset(0)] private readonly MethodTable* m_pMTOfEnclosingClass;

		// unsigned m_mb                  	: 24;
		// unsigned m_isStatic            	: 1;
		// unsigned m_isThreadLocal       	: 1;
		// unsigned m_isRVA               	: 1;
		// unsigned m_prot                	: 3;
		// unsigned m_requiresFullMbValue 	: 1;

		[FieldOffset(8)] private unsigned m_dword1;

		// unsigned m_dwOffset         		: 27;
		// unsigned m_type             		: 5;
		[FieldOffset(12)] private unsigned m_dword2;

		// todo: don't know if these work for bitfields?

		/// <summary>
		/// MemberDef
		/// </summary>
		public unsigned MB {
			get => m_dword1 & 0x18;
		}

		/// <summary>
		/// Offset in heap memory
		/// </summary>
		public unsigned Offset {
			// 27 bits
			get => m_dword2 & 0x1B;
		}

		/// <summary>
		/// Field type
		/// </summary>
		private int Type {
			get => Memory.ReadBits(m_dword2, 27, 5);
		}

		/// <summary>
		/// Whether the field is static
		/// </summary>
		public bool IsStatic => Memory.ReadBit(m_dword1, 24);

		/// <summary>
		/// Whether the field is decorated with a ThreadStatic attribute
		/// </summary>
		public bool IsThreadLocal => Memory.ReadBit(m_dword1, 25);

		/// <summary>
		/// Unknown
		/// </summary>
		public bool IsRVA => Memory.ReadBit(m_dword1, 26);

		/// <summary>
		/// Access level
		/// </summary>
		public int Protection => Memory.ReadBits(m_dword1, 26, 3);


		public bool RequiresFullMBValue => Memory.ReadBit(m_dword1, 31);

		public override string ToString()
		{
			var table = new ConsoleTable("Field", "Value");
			table.AddRow("MethodTable", Hex.ToHex(m_pMTOfEnclosingClass));
			//table.AddRow("unsigned 1", m_dword1);
			//table.AddRow("unsigned 2", m_dword2);

			// Unsigned 1
			table.AddRow("MB", MB);
			table.AddRow("Offset", Offset);
			table.AddRow("Type", Memory.ReadBits(m_dword2, 27, 5));
			table.AddRow("Type", (CorElementType) Memory.ReadBits(m_dword2, 27, 5));

			table.AddRow("Static", IsStatic);
			table.AddRow("ThreadLocal", IsThreadLocal);
			table.AddRow("RVA", IsRVA);
			table.AddRow("Protection", Protection);
			table.AddRow("Requires full MB value", RequiresFullMBValue);

			return table.ToMarkDownString();
		}

	}

	/// <summary>
	/// Source: https://github.com/dotnet/coreclr/blob/f31097f14560b193e76a7b2e1e61af9870b5356b/src/System.Private.CoreLib/src/System/Reflection/MdImport.cs#L22
	/// Source 2: https://github.com/dotnet/coreclr/blob/7b169b9a7ed2e0e1eeb668e9f1c2a049ec34ca66/src/inc/corhdr.h#L863
	/// For sizes: https://github.com/dotnet/coreclr/blob/de586767f51432e5d89f6fcffee07c488fdeeb7b/src/vm/siginfo.cpp#L63
	/// </summary>
	internal enum CorElementType : byte
	{
		End         = 0x00,
		Void        = 0x01,
		Boolean     = 0x02,
		Char        = 0x03,
		I1          = 0x04,
		U1          = 0x05,
		I2          = 0x06,
		U2          = 0x07,
		I4          = 0x08,
		U4          = 0x09,
		I8          = 0x0A,
		U8          = 0x0B,
		R4          = 0x0C,
		R8          = 0x0D,
		String      = 0x0E,
		Ptr         = 0x0F,
		ByRef       = 0x10,
		ValueType   = 0x11,
		Class       = 0x12,
		Var         = 0x13,
		Array       = 0x14,
		GenericInst = 0x15,
		TypedByRef  = 0x16,
		I           = 0x18,
		U           = 0x19,
		FnPtr       = 0x1B,
		Object      = 0x1C,
		SzArray     = 0x1D,
		MVar        = 0x1E,
		CModReqd    = 0x1F,
		CModOpt     = 0x20,
		Internal    = 0x21,
		Max         = 0x22,
		Modifier    = 0x40,
		Sentinel    = 0x41,
		Pinned      = 0x45,
	}

	internal enum MbMask
	{
		PackedMbLayoutMbMask       = 0x01FFFF,
		PackedMbLayoutNameHashMask = 0xFE0000
	}

}