using System;
using System.Runtime.InteropServices;
using RazorCommon;
using static RazorSharp.Unsafe;

namespace RazorSharp.Runtime
{

	using DWORD = UInt32;
	using WORD = UInt16;

	// Union
	[StructLayout(LayoutKind.Explicit)]
	internal unsafe struct EEClassPtr
	{
		[FieldOffset(0)] internal void* m_pEEClass;
		[FieldOffset(8)] internal void* m_pCanonMT;

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
		[FieldOffset(8)] internal void* m_pMultipurposeSlot2;
	}

	// Union
	[StructLayout(LayoutKind.Explicit)]
	internal unsafe struct InstSlot
	{
		[FieldOffset(0)]  void* m_pPerInstInfo;
		[FieldOffset(8)]  void* m_ElementTypeHnd;
		[FieldOffset(16)] void* m_pMultipurposeSlot1;
	}

	//https://github.com/dotnet/coreclr/blob/61146b5c5851698e113e936d4e4b51b628095f27/src/vm/methodtable.h
	//https://github.com/dotnet/coreclr/blob/db55a1decc1d02538e61eac7db80b7daa351d5b6/src/gc/env/gcenv.object.h

	// todo: WIP
	[StructLayout(LayoutKind.Explicit)]
	internal unsafe struct EEClass
	{
		[FieldOffset(0)] private void* m_pGuidInfo;

		[FieldOffset(8)]  private void* m_rpOptionalFields;
		[FieldOffset(16)] private void* m_pMethodTable;
		[FieldOffset(24)] private void* m_pFieldDescList;
		[FieldOffset(32)] private void* m_pChunks;

		[FieldOffset(40)]

		// Union
		private uint m_cbNativeSize;

//#ifdef FEATURE_COMINTEROP
		//ComCallWrapperTemplate * m_pccwTemplate; // points to interop data structures used when this type is exposed to COM
//#endif                                               // FEATURE_COMINTEROP

		[FieldOffset(44)] private void* m_pccwTemplate;
		[FieldOffset(52)] private DWORD m_dwAttrClass;
		[FieldOffset(56)] private DWORD m_VMFlags;

		// Line 1942...

		public override string ToString()
		{
			return $"m_dwAttrClass: {m_dwAttrClass} m_VMFlags: {m_VMFlags}";
		}
	}

	[StructLayout(LayoutKind.Explicit)]
	public unsafe struct MethodTable
	{
		/*[FieldOffset(0)]
		private GCInfo gc;*/

		#region Properties and Accessors

		#region Flags

		public DWORD Flags {
			get {
				var dwPtr = AddressOf(ref m_dwFlags);
				return *(DWORD*) dwPtr;
			}
		}

		public MethodTableFlags TableFlags => (MethodTableFlags) Flags;

		public WORD LowFlags => m_dwFlags.m_flags;

		public MethodTableFlagsLow TableFlagsLow => (MethodTableFlagsLow) LowFlags;

		public WORD Flags2 => m_wFlags2;

		public MethodTableFlags2 TableFlags2 => (MethodTableFlags2) Flags2;

		#endregion

		public WORD ComponentSize {
			get {
				if (HasComponentSize)
					return m_dwFlags.m_componentSize;
				else return 0;
			}
		}

		public DWORD BaseSize => m_BaseSize;

		public WORD Token => m_wToken;

		public WORD NumVirtuals => m_wNumVirtuals;

		public WORD NumInterfaces => m_wNumInterfaces;

		public MethodTable* Parent => m_pParentMethodTable;

		public void* Module => m_pLoaderModule;

		public void*        EEClass => _eeClassPtr.m_pEEClass;
		public MethodTable* Canon   => (MethodTable*) _eeClassPtr.m_pCanonMT;

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

		[FieldOffset(0)] private DWFlags m_dwFlags;

		// Base size of instance of this class when allocated on the heap
		[FieldOffset(4)] private DWORD m_BaseSize;

		[FieldOffset(8)] private WORD m_wFlags2;

		// Class token if it fits into 16-bits. If this is (WORD)-1, the class token is stored in the TokenOverflow optional member.
		[FieldOffset(10)] private WORD m_wToken;


		// <NICE> In the normal cases we shouldn't need a full word for each of these </NICE>
		[FieldOffset(12)] private WORD m_wNumVirtuals;


		[FieldOffset(14)] private WORD m_wNumInterfaces;


		// On Linux ARM is a RelativeFixupPointer. Otherwise,
		// Parent PTR_MethodTable if enum_flag_HasIndirectParent is not set. Pointer to indirection cell
		// if enum_flag_enum_flag_HasIndirectParent is set. The indirection is offset by offsetof(MethodTable, m_pParentMethodTable).
		// It allows casting helpers to go through parent chain natually. Casting helper do not need need the explicit check
		// for enum_flag_HasIndirectParentMethodTable.
		[FieldOffset(16)] private MethodTable* m_pParentMethodTable;


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


		[FieldOffset(32)] private EEClassPtr _eeClassPtr;


		// m_pPerInstInfo and m_pInterfaceMap have to be at fixed offsets because of performance sensitive
		// JITed code and JIT helpers. However, they are frequently not present. The space is used by other
		// multipurpose slots on first come first served basis if the fixed ones are not present. The other
		// multipurpose are DispatchMapSlot, NonVirtualSlots, ModuleOverride (see enum_flag_MultipurposeSlotsMask).
		// The multipurpose slots that do not fit are stored after vtable slots.

		//private InstSlot _instSlot;

		//private MapSlot _mapSlot;

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

			//table.AddRow("Flags", $"{Flags} ({TableFlags})");

			//todo: this seems to read duplicates as some enums are the same ushort value
			//table.AddRow("Flags (low)", string.Format("{0} ({1})", Flags, Collections.ToString(Constants.ExtractLow(m_dwFlags.m_flags))));

			table.AddRow("Low flags", $"{LowFlags} ({TableFlagsLow})");

			table.AddRow("Flags 2",
				string.Format("{0} ({1})", Flags2, Collections.ToString(Constants.Extract(Flags2))));

			//table.AddRow("Flags 2", $"{Flags2} ({TableFlags2})");
			table.AddRow("Token", m_wToken);
			table.AddRow("Number virtuals", m_wNumVirtuals);
			table.AddRow("Number interfaces", m_wNumInterfaces);

			table.AddRow("Parent MT", Hex.ToHex(m_pParentMethodTable));
			table.AddRow("Module", Hex.ToHex(m_pLoaderModule));

			table.AddRow("EEClass", Hex.ToHex(_eeClassPtr.m_pEEClass));
			table.AddRow("Canon MT", Hex.ToHex(_eeClassPtr.m_pCanonMT));


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
				bool eeEq      = _eeClassPtr.Equals(mtOther._eeClassPtr);

				return dwFlagsEq && sizesEq && flags2Eq && numEq && tokenEq && parentEq && moduleEq && eeEq;
			}

			return false;
		}
	}

}