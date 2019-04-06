#region

using System;
using System.Runtime.InteropServices;

#endregion

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace RazorSharp.Native.ThreadContext
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct M128A
	{
		public ulong High;
		public long  Low;

		public override string ToString()
		{
			return String.Format("High: {0}, Low: {1}", High, Low);
		}
	}
}