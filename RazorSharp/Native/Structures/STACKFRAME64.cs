using System;
using System.Runtime.InteropServices;
using RazorSharp.Common;

namespace RazorSharp.Native.Structures
{

	[StructLayout(LayoutKind.Sequential)]
	public struct KDHELP64
	{
		public ulong Thread;
		public uint  ThCallbackStack;
		public uint  ThCallbackBStore;
		public uint  NextCallback;
		public uint  FramePointer;
		public ulong KiCallUserMode;
		public ulong KeUserCallbackDispatcher;
		public ulong SystemRangeStart;
		public ulong KiUserExceptionDispatcher;
		public ulong StackBase;
		public ulong StackLimit;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
		public ulong[] Reserved;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct ADDRESS64
	{
		public ulong       Offset;
		public ushort      Segment;
		public AddressMode Mode;

		public override string ToString()
		{
			var table = new ConsoleTable("Offset", "Segment", "Mode");
			table.AddRow(Hex.ToHex(Offset), Segment, Mode);
			return table.ToMarkDownString();
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct STACKFRAME64
	{
		public ADDRESS64 AddrPC;         //Program Counter EIP, RIP
		public ADDRESS64 AddrReturn;     //Return Address
		public ADDRESS64 AddrFrame;      //Frame Pointer EBP, RBP or RDI
		public ADDRESS64 AddrStack;      //Stack Pointer ESP, RSP
		public ADDRESS64 AddrBStore;     //IA64 Backing Store RsBSP
		public IntPtr    FuncTableEntry; //x86 = FPO_DATA struct, if none = NULL

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		public ulong[] Params; //possible arguments to the function

		public bool Far;     //TRUE if this is a WOW far call
		public bool Virtual; //TRUE if this is a virtual frame

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
		public ulong[] Reserved; //used internally by StackWalk64

		public KDHELP64 KdHelp; //specifies helper data for walking kernel callback frames

		public override string ToString()
		{
			var table = new ConsoleTable("Field", "Value");
			table.AddRow("AddrPC", Hex.ToHex(AddrPC.Offset));
			table.AddRow("AddrReturn", Hex.ToHex(AddrReturn.Offset));
			table.AddRow("AddrFrame", Hex.ToHex(AddrFrame.Offset));
			table.AddRow("AddrStack", Hex.ToHex(AddrStack.Offset));
			table.AddRow("AddrBStore", Hex.ToHex(AddrBStore.Offset));
			table.AddRow("FuncTableEntry", Hex.ToHex(FuncTableEntry));
			for (int i = 0; i < Params.Length; i++) {
				table.AddRow(String.Format("Params[{0}]",i), Params[i]);
			}
			table.AddRow("Far", Far);
			table.AddRow("Virtual",Virtual);

			return table.ToMarkDownString();
		}
	}

}