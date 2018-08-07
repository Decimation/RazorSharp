using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using RazorCommon;

// ReSharper disable InconsistentNaming

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
		// This actually only seems to be a byte?
		[FieldOffset(0)] private readonly MethodTable* m_pMTOfEnclosingClass;

		// unsigned m_mb                  	: 24;
		// unsigned m_isStatic            	: 1;
		// unsigned m_isThreadLocal       	: 1;
		// unsigned m_isRVA               	: 1;
		// unsigned m_prot                	: 3;
		// unsigned m_requiresFullMbValue 	: 1;

		[FieldOffset(8)] private readonly unsigned m_dword1;

		// unsigned m_dwOffset         		: 27;
		// unsigned m_type             		: 5;
		[FieldOffset(12)] private readonly unsigned m_dword2;

		/// <summary>
		/// MemberDef
		/// </summary>
		public int MB {
			//get => Memory.ReadBits(m_dword1, 0, 24);
			get => (int) (m_dword1 & 0xFFFFFF);
		}

		/// <summary>
		/// Offset in heap memory
		/// </summary>
		public int Offset {
			//get { return Memory.ReadBits(m_dword2, 0, 27); }
			get => (int) (m_dword2 & 0x7FFFFFF);
		}

		/// <summary>
		/// Field type
		/// </summary>
		private int Type {
			get => (int) ((m_dword2 >> 27) & 0x7FFFFFF);
		}

		public CorElementType CorType {
			get => (CorElementType) Type;
		}

		/// <summary>
		/// Whether the field is static
		/// </summary>
		public bool IsStatic {
			get { return Memory.ReadBit(m_dword1, 24); }
		}

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
		public int Protection {
			//get { return Memory.ReadBits(m_dword1, 26, 3); }
			get => (int) ((m_dword1 >> 26) & 0x3FFFFFF);
		}

		public int Size {
			//todo: get size of -1
			get {
				int s = Constants.SizeOfCorElementType(CorType);
				return s;
			}
		}



		/// <summary>
		/// Slower than using Reflection
		/// </summary>
		public string Name {
			get {
				fixed (FieldDesc* __this = &this) {
					byte* lpcutf8 = CLRFunctions.FieldDescFunctions.GetName(__this);
					return CLRFunctions.StringFunctions.NewString(lpcutf8);
				}
			}
		}

		public bool RequiresFullMBValue => Memory.ReadBit(m_dword1, 31);

		public override string ToString()
		{
			var table = new ConsoleTable("Field", "Value");
			table.AddRow("MethodTable", Hex.ToHex(m_pMTOfEnclosingClass));

			// Unsigned 1
			table.AddRow("MB", MB);
			table.AddRow("Offset", Offset);
			table.AddRow("CorType", CorType);
			table.AddRow("Size", Size);

			table.AddRow("Static", IsStatic);
			table.AddRow("ThreadLocal", IsThreadLocal);
			table.AddRow("RVA", IsRVA);
			table.AddRow("Protection", Protection);
			table.AddRow("Requires full MB value", RequiresFullMBValue);

			return table.ToMarkDownString();
		}

	}


	internal enum MbMask
	{
		PackedMbLayoutMbMask       = 0x01FFFF,
		PackedMbLayoutNameHashMask = 0xFE0000
	}

}