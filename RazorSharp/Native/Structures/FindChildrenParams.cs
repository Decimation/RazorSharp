using System;
using System.Runtime.InteropServices;

namespace RazorSharp.Native.Structures
{
	/// <summary>
	/// Native name: TI_FINDCHILDREN_PARAMS
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct FindChildrenParams
	{
		public readonly UInt32 Count;
		public readonly UInt32 Start;
		public readonly UInt32 ChildId;
	}
}