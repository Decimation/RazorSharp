#region

using System;
using System.Runtime.InteropServices;
using RazorSharp.Native.Win32.Enums;
using SimpleSharp;
using SimpleSharp.Strings;

// ReSharper disable NonReadonlyMemberInGetHashCode

// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable MemberCanBePrivate.Global

#endregion

// ReSharper disable UnassignedReadonlyField

namespace RazorSharp.Native.Win32.Structures
{
	/// <summary>
	///     Contains information about a range of pages in the virtual address space of a process.
	///     The <see cref="Kernel32.VirtualQuery(System.IntPtr)" />,
	///     <see cref="Kernel32.VirtualQuery(IntPtr, ref MemoryBasicInformation, uint)" /> and VirtualQueryEx functions use
	///     this structure.
	///     <a href="https://docs.microsoft.com/en-us/windows/desktop/api/winnt/ns-winnt-_memory_basic_information">
	///         Doc
	///     </a>
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct MemoryBasicInformation : IEquatable<MemoryBasicInformation>
	{
		/// <summary>
		///     A pointer to the base address of the region of pages.
		/// </summary>
		public IntPtr BaseAddress;

		/// <summary>
		///     A pointer to the base address of a range of pages allocated by the <see cref="Kernel32.VirtualAlloc" /> function.
		///     The page pointed to by the <see cref="BaseAddress" /> member is contained within this allocation range.
		/// </summary>
		public IntPtr AllocationBase;

		/// <summary>
		///     The memory protection option when the region was initially allocated. This member can be one of the memory
		///     protection constants or 0 if the caller does not have access.
		/// </summary>
		public MemoryProtection AllocationProtect;

		/// <summary>
		///     The size of the region beginning at the base address in which all pages have identical attributes, in bytes.
		/// </summary>
		public IntPtr RegionSize;

		/// <summary>
		///     The state of the pages in the region.
		/// </summary>
		public MemState State;

		/// <summary>
		///     The access protection of the pages in the region. This member is one of the values listed for the
		///     <see cref="AllocationProtect" /> member.
		/// </summary>
		public MemoryProtection Protect;

		/// <summary>
		///     The type of pages in the region.
		/// </summary>
		public MemType Type;

		public bool IsReadable => Protect.HasFlag(MemoryProtection.ExecuteRead)
		                          || Protect.HasFlag(MemoryProtection.ReadOnly)
		                          || Protect.HasFlag(MemoryProtection.ReadWrite)
		                          || Protect.HasFlag(MemoryProtection.ExecuteWriteCopy);

		public bool Equals(MemoryBasicInformation other)
		{
			return BaseAddress.Equals(other.BaseAddress) && AllocationBase.Equals(other.AllocationBase) &&
			       AllocationProtect == other.AllocationProtect && RegionSize.Equals(other.RegionSize) &&
			       State == other.State && Protect == other.Protect && Type == other.Type;
		}

		public override bool Equals(object obj)
		{
			return obj is MemoryBasicInformation other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked {
				int hashCode = BaseAddress.GetHashCode();
				hashCode = (hashCode * 397) ^ AllocationBase.GetHashCode();
				hashCode = (hashCode * 397) ^ (int) AllocationProtect;
				hashCode = (hashCode * 397) ^ RegionSize.GetHashCode();
				hashCode = (hashCode * 397) ^ (int) State;
				hashCode = (hashCode * 397) ^ (int) Protect;
				hashCode = (hashCode * 397) ^ (int) Type;
				return hashCode;
			}
		}

		public static bool operator ==(MemoryBasicInformation left, MemoryBasicInformation right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(MemoryBasicInformation left, MemoryBasicInformation right)
		{
			return !left.Equals(right);
		}

		public ConsoleTable ToTable()
		{
			var table = new ConsoleTable("Field", "Value");
			table.AddRow("Base address", Hex.ToHex(BaseAddress));
			table.AddRow("Allocation base", Hex.ToHex(AllocationBase));
			table.AddRow("Allocation protect", AllocationProtect);
			table.AddRow("Region size", RegionSize);
			table.AddRow("State", State);
			table.AddRow("Protect", Protect);
			table.AddRow("Type", Type);
			return table;
		}

		public override string ToString()
		{
			return String.Format("State: {0} | Protection: {1} | Type: {2} | Allocation: {3}",
			                     State, Protect, Type, AllocationProtect);
		}
	}
}