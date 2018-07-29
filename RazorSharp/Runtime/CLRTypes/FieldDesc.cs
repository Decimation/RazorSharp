using System;
using System.Runtime.InteropServices;
using RazorCommon;

namespace RazorSharp.Runtime.CLRTypes
{

	using unsigned = UInt32;

	//todo: fix
	/// <summary>
	/// https://github.com/dotnet/coreclr/blob/59714b683f40fac869050ca08acc5503e84dc776/src/vm/field.cpp
	/// Source: https://github.com/dotnet/coreclr/blob/59714b683f40fac869050ca08acc5503e84dc776/src/vm/field.h#L43
	/// </summary>
	[StructLayout(LayoutKind.Explicit)]
	public unsafe struct FieldDesc
	{
		[FieldOffset(0)]  private readonly MethodTable* m_pMTOfEnclosingClass;

		// unsigned m_mb                  	: 24;
		// unsigned m_isStatic            	: 1;
		// unsigned m_isThreadLocal       	: 1;
		// unsigned m_isRVA               	: 1;
		// unsigned m_prot                	: 3;
		// unsigned m_requiresFullMbValue 	: 1;

		[FieldOffset(8)]  private unsigned     m_dword1;

		// unsigned m_dwOffset         		: 27;
		// unsigned m_type             		: 5;
		[FieldOffset(12)] private unsigned     m_dword2;

		// todo: don't know if these work for bitfields?

		private unsigned MB {
			get => m_dword1 & 0x18;
		}

		private unsigned IsStatic {
			get => (m_dword1 >> 24) & 0x18;
		}

		private unsigned Offset {
			// 27 bits
			get => m_dword2 & 0x1B;
		}

		private unsigned Type {
			get => (m_dword2 >> 5) & 0x1B;
		}

		public override string ToString()
		{
			var table = new ConsoleTable("Field", "Value");
			table.AddRow("MethodTable",Hex.ToHex(m_pMTOfEnclosingClass));
			table.AddRow("unsigned 1", m_dword1);
			table.AddRow("unsigned 2", m_dword2);
			table.AddRow("Offset",Offset);
			table.AddRow("Type",Type);
			table.AddRow("Static",Memory.Memory.ReadBit((int) m_dword1,24));



			return table.ToMarkDownString();
		}
	}

}