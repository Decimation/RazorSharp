#region

using System;
using System.Runtime.InteropServices;
using RazorCommon;

// ReSharper disable BuiltInTypeReferenceStyle
// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable InconsistentNaming

#endregion


namespace RazorSharp.CLR.Structures
{

	#region

	using WORD = UInt16;

	#endregion

	/// <summary>
	///     <para>
	///         Low <c>WORD</c> is component size for array and string types (<see cref="MethodTable.HasComponentSize" />
	///         returns <c>true</c>).
	///     </para>
	///     <para>Used for flags otherwise.</para>
	/// </summary>
	[StructLayout(LayoutKind.Explicit)]
	internal struct DWFlags
	{

		#region Fields

		[FieldOffset(0)] private readonly WORD m_componentSize;
		[FieldOffset(2)] private readonly WORD m_flags;

		#endregion


		/// <summary>
		///     <para>The size of one component (or element).</para>
		///     <para>Applicable only to strings and arrays.</para>
		/// </summary>
		internal WORD ComponentSize => m_componentSize;

		/// <summary>
		///     Flags
		/// </summary>
		internal WORD Flags => m_flags;

		public override string ToString()
		{
			ConsoleTable table = new ConsoleTable("Field", "Value");
			table.AddRow("Component size", m_componentSize);
			table.AddRow("Flags", m_flags);

			return table.ToStringAlternative();
		}

		#region Equality

		public override bool Equals(object obj)
		{
			if (obj?.GetType() == GetType()) {
				DWFlags dwOther = (DWFlags) obj;
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

		#endregion

	}

}