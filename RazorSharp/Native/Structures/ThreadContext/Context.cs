#region

using System.Runtime.InteropServices;

#endregion

// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable MemberCanBePrivate.Global

namespace RazorSharp.Native.Structures.ThreadContext
{
	[StructLayout(LayoutKind.Sequential)]
	public struct Context
	{
		public uint ContextFlags; //set this to an appropriate value 

		// Retrieved by CONTEXT_DEBUG_REGISTERS 
		public uint Dr0;
		public uint Dr1;
		public uint Dr2;
		public uint Dr3;
		public uint Dr6;

		public uint Dr7;

		// Retrieved by CONTEXT_FLOATING_POINT 
		public FloatingSaveArea FloatSave;

		// Retrieved by CONTEXT_SEGMENTS 
		public uint SegGs;
		public uint SegFs;
		public uint SegEs;

		public uint SegDs;

		// Retrieved by CONTEXT_INTEGER 
		public uint Edi;
		public uint Esi;
		public uint Ebx;
		public uint Edx;
		public uint Ecx;

		public uint Eax;

		// Retrieved by CONTEXT_CONTROL 
		public uint Ebp;
		public uint Eip;
		public uint SegCs;
		public uint EFlags;
		public uint Esp;
		public uint SegSs;

		// Retrieved by CONTEXT_EXTENDED_REGISTERS 
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
		public byte[] ExtendedRegisters;
	}
}