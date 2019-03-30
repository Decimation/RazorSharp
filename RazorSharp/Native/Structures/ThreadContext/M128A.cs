using System;
using System.Runtime.InteropServices;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace RazorSharp.Native.Structures.ThreadContext
{
	[StructLayout(LayoutKind.Sequential)]
	public struct M128A
	{
		public ulong High;
		public long  Low;

		public override string ToString()
		{
			return String.Format("High: {0}, Low: {1}", this.High, this.Low);
		}
	}
}