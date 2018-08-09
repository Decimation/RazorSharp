#region

using System;
using System.Runtime.InteropServices;
using RazorCommon;

#endregion

namespace RazorSharp.Runtime.CLRTypes
{

	#region

	using DWORD = UInt32;
	using WORD = UInt16;

	#endregion

	[StructLayout(LayoutKind.Explicit)]

	// ReSharper disable once InconsistentNaming
	internal struct DWFlags
	{
		[FieldOffset(0)] private readonly WORD m_componentSize;
		[FieldOffset(2)] private readonly WORD m_flags;

		internal WORD ComponentSize => m_componentSize;
		internal WORD Flags         => m_flags;

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

		public bool Equals(DWFlags other)
		{
			return m_componentSize == other.m_componentSize && m_flags == other.m_flags;
		}

		public override int GetHashCode()
		{
			unchecked {
				return (m_componentSize.GetHashCode() * 397) ^ m_flags.GetHashCode();
			}
		}

		public static bool operator ==(DWFlags left, DWFlags right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(DWFlags left, DWFlags right)
		{
			return !left.Equals(right);
		}
	}

}