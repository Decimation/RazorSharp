using System;
using System.Runtime.InteropServices;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace RazorSharp.Native.Structures.Symbols
{
	/// <summary>
	/// Native name: TI_FINDCHILDREN_PARAMS
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct FindChildrenParams
	{
		public UInt32 Count;
		public UInt32 Start;
		public UInt32 ChildId;
	}
}