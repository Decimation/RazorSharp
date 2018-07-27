using System;
using System.Runtime.InteropServices;
using RazorCommon;

namespace RazorSharp.Runtime.CLRTypes
{

	using DWORD = UInt32;
	using WORD = UInt16;

	// Union
	[StructLayout(LayoutKind.Explicit)]
	internal unsafe struct EEClassPtr
	{
		[FieldOffset(0)] internal EEClass*     m_pEEClass;
		[FieldOffset(0)] internal MethodTable* m_pCanonMT;

		public override string ToString()
		{
			return $"{nameof(m_pEEClass)}: {Hex.ToHex(m_pEEClass)}, {nameof(m_pCanonMT)}: {Hex.ToHex(m_pCanonMT)}";
		}

		public override bool Equals(object obj)
		{
			if (obj.GetType() == this.GetType()) {
				var eeOther = (EEClassPtr) obj;
				return m_pEEClass == eeOther.m_pEEClass && m_pCanonMT == eeOther.m_pCanonMT;
			}

			return false;
		}
	}


	// Union
	[StructLayout(LayoutKind.Explicit)]
	internal struct DWFlags
	{
		[FieldOffset(0)] internal WORD m_componentSize;
		[FieldOffset(2)] internal WORD m_flags;

		public override string ToString()
		{
			var table = new ConsoleTable("Field", "Value");
			table.AddRow("Component size", m_componentSize);
			table.AddRow("Flags", m_flags);

			return table.ToStringAlternative();
		}

		public override bool Equals(object obj)
		{
			if (obj.GetType() == this.GetType()) {
				var dwOther = (DWFlags) obj;
				return m_componentSize == dwOther.m_componentSize && m_flags == dwOther.m_flags;
			}

			return false;
		}
	}

	// Union
	[StructLayout(LayoutKind.Explicit)]
	internal unsafe struct MapSlot
	{
		[FieldOffset(0)] internal void* m_pInterfaceMap;
		[FieldOffset(0)] internal ulong m_pMultipurposeSlot2;
	}

	// Union
	[StructLayout(LayoutKind.Explicit)]
	internal unsafe struct InstSlot
	{
		[FieldOffset(0)] internal void* m_pPerInstInfo;
		[FieldOffset(0)] internal ulong m_ElementTypeHnd;
		[FieldOffset(0)] internal ulong m_pMultipurposeSlot1;
	}


	//https://github.com/dotnet/coreclr/blob/61146b5c5851698e113e936d4e4b51b628095f27/src/vm/methodtable.h
	//https://github.com/dotnet/coreclr/blob/db55a1decc1d02538e61eac7db80b7daa351d5b6/src/gc/env/gcenv.object.h


	//todo: fix
	// field.h
	[StructLayout(LayoutKind.Explicit)]
	internal unsafe struct FieldDesc
	{
		[FieldOffset(0)] private MethodTable* m_pMTOfEnclosingClass;
		[FieldOffset(8)] private uint         m_fields;

		public override string ToString()
		{
			return String.Format("m_pMTOfEnclosingClass: {0}, m_fields: {1}", Hex.ToHex(m_pMTOfEnclosingClass),
				m_fields);
		}
	}


	[StructLayout(LayoutKind.Explicit)]
	public unsafe struct MethodTable
	{
		/*[FieldOffset(-??)]
		private GCInfo gc;*/

		#region Properties and Accessors

		#region Flags

		public DWORD Flags {
			get {
				var dwPtr = Unsafe.AddressOf(ref m_dwFlags);
				return *(DWORD*) dwPtr;
			}
		}

		public MethodTableFlags TableFlags => (MethodTableFlags) Flags;

		public WORD LowFlags => m_dwFlags.m_flags;

		public MethodTableFlagsLow TableFlagsLow => (MethodTableFlagsLow) LowFlags;

		public WORD Flags2 => m_wFlags2;

		public MethodTableFlags2 TableFlags2 => (MethodTableFlags2) Flags2;

		#endregion

		/// <summary>
		/// The size of an individual element when this type is an array or string.
		///
		/// This size will be 2 with strings (sizeof(char)).
		/// </summary>
		public WORD ComponentSize {
			get {
				if (HasComponentSize)
					return m_dwFlags.m_componentSize;
				else return 0;
			}
		}

		/// <summary>
		/// The base size of this class when allocated on the heap.
		/// </summary>
		public DWORD BaseSize => m_BaseSize;

		public WORD Token => m_wToken;

		public WORD NumVirtuals => m_wNumVirtuals;

		public WORD NumInterfaces => m_wNumInterfaces;

		public MethodTable* Parent => m_pParentMethodTable;

		public void* Module => m_pLoaderModule;

		public EEClass*     EEClass => m_pEEClass;
		public MethodTable* Canon   => m_pCanonMT;

		//public FieldDesc* FieldDescList => _eeClassPtr.m_pEEClass->m_pFieldDescList;

		public bool HasComponentSize {
			get {
				// Note that we can't just check m_componentSize != 0 here. The VM
				// may still construct a method table that does not have a component
				// size, according to this method, but still has a number in the low
				// 16 bits of the method table flags parameter.
				//
				// The solution here is to do what the VM does and check the
				// HasComponentSize flag so that we're on the same page.
				return (TableFlags & MethodTableFlags.HasComponentSize) != 0;
			}
		}

		#endregion

		#region Fields

		//** Status: verified
		[FieldOffset(0)] private DWFlags m_dwFlags;

		//** Status: verified
		[FieldOffset(4)] private DWORD m_BaseSize;

		//** Status: unknown
		[FieldOffset(8)] private WORD m_wFlags2;

		//** Status: unknown
		// Class token if it fits into 16-bits. If this is (WORD)-1, the class token is stored in the TokenOverflow optional member.
		[FieldOffset(10)] private WORD m_wToken;

		//** Status: unknown
		[FieldOffset(12)] private WORD m_wNumVirtuals;

		//** Status: verified
		[FieldOffset(14)] private WORD m_wNumInterfaces;

		//** Status: verified
		// On Linux ARM is a RelativeFixupPointer. Otherwise,
		// Parent PTR_MethodTable if enum_flag_HasIndirectParent is not set. Pointer to indirection cell
		// if enum_flag_enum_flag_HasIndirectParent is set. The indirection is offset by offsetof(MethodTable, m_pParentMethodTable).
		// It allows casting helpers to go through parent chain natually. Casting helper do not need need the explicit check
		// for enum_flag_HasIndirectParentMethodTable.
		[FieldOffset(16)] private MethodTable* m_pParentMethodTable;

		//** Status: verified
		[FieldOffset(24)] private void* m_pLoaderModule; // LoaderModule. It is equal to the ZapModule in ngened images

		//todo - lowest two bits of what?
		// The value of lowest two bits describe what the union contains
		enum LowBits
		{
			UNION_EECLASS     = 0, //  0 - pointer to EEClass. This MethodTable is the canonical method table.
			UNION_INVALID     = 1, //  1 - not used
			UNION_METHODTABLE = 2, //  2 - pointer to canonical MethodTable.
			UNION_INDIRECTION = 3  //  3 - pointer to indirection cell that points to canonical MethodTable.
		};                         //      (used only if FEATURE_PREJIT is defined)


		//** Status: unknown
		[FieldOffset(32)] private void* m_pWriteableData;

		//** Status: verified
		[FieldOffset(40)] private EEClass*     m_pEEClass;
		//** Status: verified
		[FieldOffset(40)] private MethodTable* m_pCanonMT;

		//** Status: unknown
		[FieldOffset(48)] private InstSlot m_slotInfo;

		//** Status: unknown
		[FieldOffset(56)] private MapSlot m_mapSlot;

		// m_pPerInstInfo and m_pInterfaceMap have to be at fixed offsets because of performance sensitive
		// JITed code and JIT helpers. However, they are frequently not present. The space is used by other
		// multipurpose slots on first come first served basis if the fixed ones are not present. The other
		// multipurpose are DispatchMapSlot, NonVirtualSlots, ModuleOverride (see enum_flag_MultipurposeSlotsMask).
		// The multipurpose slots that do not fit are stored after vtable slots.

		// VTable and Non-Virtual slots go here

		// Overflow multipurpose slots go here

		// Optional Members go here
		//    See above for the list of optional members

		// Generic dictionary pointers go here

		// Interface map goes here

		// Generic instantiation+dictionary goes here

		#endregion

		public override string ToString()
		{
			var table = new ConsoleTable("Field", "Value");
			if (HasComponentSize)
				table.AddRow("Component size", m_dwFlags.m_componentSize);
			table.AddRow("Base size", m_BaseSize);

			table.AddRow("Flags", string.Format("{0} ({1})", Flags, Collections.ToString(Constants.Extract(Flags))));


			table.AddRow("Low flags", $"{LowFlags} ({TableFlagsLow})");

			table.AddRow("Flags 2",
				string.Format("{0} ({1})", Flags2, Collections.ToString(Constants.Extract(Flags2))));

			table.AddRow("Token", m_wToken);
			table.AddRow("Number virtuals", m_wNumVirtuals);
			table.AddRow("Number interfaces", m_wNumInterfaces);

			table.AddRow("Parent MT", Hex.ToHex(m_pParentMethodTable));
			table.AddRow("Module", Hex.ToHex(m_pLoaderModule));

			table.AddRow("m_pWriteableData", Hex.ToHex(m_pWriteableData));

			table.AddRow("EEClass", Hex.ToHex(m_pEEClass));

			table.AddRow("Canon MT", Hex.ToHex(m_pCanonMT));

			table.AddRow("m_ElementTypeHnd", (m_slotInfo.m_ElementTypeHnd));
			table.AddRow("m_pMultipurposeSlot1", (m_slotInfo.m_pMultipurposeSlot1));
			table.AddRow("m_pPerInstInfo", Hex.ToHex(m_slotInfo.m_pPerInstInfo));
			table.AddRow("m_pInterfaceMap", Hex.ToHex(m_mapSlot.m_pInterfaceMap));
			table.AddRow("m_pMultipurposeSlot2", (m_mapSlot.m_pMultipurposeSlot2));
			return table.ToMarkDownString();
		}

		public static bool operator ==(MethodTable a, MethodTable b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(MethodTable a, MethodTable b)
		{
			return !a.Equals(b);
		}


		public override bool Equals(object obj)
		{
			if (obj.GetType() == this.GetType()) {
				var  mtOther   = (MethodTable) obj;
				bool dwFlagsEq = m_dwFlags.Equals(mtOther.m_dwFlags);
				bool sizesEq   = m_BaseSize == mtOther.m_BaseSize && ComponentSize == mtOther.ComponentSize;
				bool flags2Eq  = m_wFlags2 == mtOther.m_wFlags2;
				bool numEq     = NumInterfaces == mtOther.NumInterfaces && NumVirtuals == mtOther.NumVirtuals;
				bool tokenEq   = Token == mtOther.Token;
				bool parentEq  = Parent == mtOther.Parent;
				bool moduleEq  = Module == mtOther.Module;
				bool eeEq = m_pEEClass == mtOther.m_pEEClass && m_pCanonMT == mtOther.m_pCanonMT;

				return dwFlagsEq && sizesEq && flags2Eq && numEq && tokenEq && parentEq && moduleEq && eeEq;
			}

			return false;
		}
	}

}