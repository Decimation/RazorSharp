#region

using System;
using System.Runtime.InteropServices;
using RazorCommon;
using RazorSharp.Pointers;
using RazorSharp.Utilities;

#endregion

// ReSharper disable InconsistentNaming

namespace RazorSharp.Runtime.CLRTypes
{

	#region

	using unsigned = UInt32;

	#endregion


	/// <summary>
	/// Source: https://github.com/dotnet/coreclr/blob/59714b683f40fac869050ca08acc5503e84dc776/src/vm/field.cpp<para></para>
	/// Source 2: https://github.com/dotnet/coreclr/blob/59714b683f40fac869050ca08acc5503e84dc776/src/vm/field.h#L43<para></para>
	/// DO NOT DEREFERENCE <para></para>
	/// Internal representation: FieldHandle.Value
	/// </summary>
	[StructLayout(LayoutKind.Explicit)]
	public unsafe struct FieldDesc
	{
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
		public int MB => (int) (m_dword1 & 0xFFFFFF);

		/// <summary>
		/// Offset in heap memory
		/// </summary>
		public int Offset => (int) (m_dword2 & 0x7FFFFFF);

		/// <summary>
		/// Field type
		/// </summary>
		private int Type => (int) ((m_dword2 >> 27) & 0x7FFFFFF);

		public CorElementType CorType {
			get => (CorElementType) Type;
		}

		/// <summary>
		/// Whether the field is static
		/// </summary>
		public bool IsStatic => Memory.Memory.ReadBit(m_dword1, 24);

		/// <summary>
		/// Whether the field is decorated with a ThreadStatic attribute
		/// </summary>
		public bool IsThreadLocal => Memory.Memory.ReadBit(m_dword1, 25);

		/// <summary>
		/// Unknown
		/// </summary>
		public bool IsRVA => Memory.Memory.ReadBit(m_dword1, 26);

		/// <summary>
		/// Access level
		/// </summary>
		private int ProtectionInt => (int) ((m_dword1 >> 26) & 0x3FFFFFF);

		public ProtectionLevel Protection => (ProtectionLevel) ProtectionInt;

		/// <summary>
		/// Address-sensitive
		/// </summary>
		public int Size {
			get {
				int s = Constants.SizeOfCorElementType(CorType);
				if (s == -1) {
					fixed (FieldDesc* __this = &this) {
						return CLRFunctions.FieldDescFunctions.LoadSize(__this);
					}
				}

				return s;
			}
		}

		/// <summary>
		/// Slower than using Reflection
		///
		/// Address-sensitive
		/// </summary>
		public string Name {
			get {
#if DEBUG_SIGSCANNING
				fixed (FieldDesc* __this = &this) {
					byte* lpcutf8 = CLRFunctions.FieldDescFunctions.GetName(__this);
					return CLRFunctions.StringFunctions.NewString(lpcutf8);
				}
#else
				return Assertion.WIPString;
#endif
			}
		}

		/// <summary>
		/// Address-sensitive
		/// </summary>
		public MethodTable* MethodTableOfEnclosingClass {
			get {
				return (MethodTable*) PointerUtils.Add(Unsafe.AddressOf(ref this).ToPointer(), m_pMTOfEnclosingClass);
			}
		}

		public bool RequiresFullMBValue => Memory.Memory.ReadBit(m_dword1, 31);

		public enum ProtectionLevel
		{
			Private           = 4,
			PrivateProtected  = 8,
			Internal          = 12,
			Protected         = 16,
			ProtectedInternal = 20,
			Public            = 24,
		}


		public override string ToString()
		{
			var table = new ConsoleTable("Field", "Value");

			// !NOTE NOTE NOTE!
			// this->ToString() must be used to view this
			// when the pointer is copied, MethodTableOfEnclosingClass
			// read from incorrect memory
			table.AddRow("Enclosing MethodTable", Hex.ToHex(MethodTableOfEnclosingClass));

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

			table.AddRow("Name", Name);

			return table.ToMarkDownString();
		}

	}


	internal enum MbMask
	{
		PackedMbLayoutMbMask       = 0x01FFFF,
		PackedMbLayoutNameHashMask = 0xFE0000
	}

}