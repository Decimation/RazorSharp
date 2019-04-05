#region

using System.Runtime.InteropServices;

#endregion

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace RazorSharp.Native.Structures.Symbols
{
	/// <summary>
	///     Native name: TI_FINDCHILDREN_PARAMS
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct FindChildrenParams
	{
		public uint Count;
		public uint Start;
		public uint ChildId;
	}
}