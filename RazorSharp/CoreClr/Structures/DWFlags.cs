#region

using System;
using System.Runtime.InteropServices;
using RazorCommon;


// ReSharper disable BuiltInTypeReferenceStyle
// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable InconsistentNaming

#endregion


namespace RazorSharp.CoreClr.Structures
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

		#endregion


		/// <summary>
		///     <para>The size of one component (or element).</para>
		///     <para>Applicable only to strings and arrays.</para>
		/// </summary>
		[field: FieldOffset(0)]
		internal WORD ComponentSize { get; }

		/// <summary>
		///     Flags
		/// </summary>
		[field: FieldOffset(2)]
		internal WORD Flags { get; }

		public override string ToString()
		{
			var table = new ConsoleTable("Field", "Value");
			table.AddRow("Component size", ComponentSize);
			table.AddRow("Flags", Flags);

			return table.ToString();
		}

		#region Equality

		public override bool Equals(object obj)
		{
			if (obj?.GetType() == GetType()) {
				var dwOther = (DWFlags) obj;
				return ComponentSize == dwOther.ComponentSize && Flags == dwOther.Flags;
			}

			return false;
		}

		public bool Equals(DWFlags other)
		{
			return ComponentSize == other.ComponentSize && Flags == other.Flags;
		}

		public override int GetHashCode()
		{
			unchecked {
				return (ComponentSize.GetHashCode() * 397) ^ Flags.GetHashCode();
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