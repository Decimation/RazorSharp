#region

using System;
using System.Runtime.InteropServices;
using RazorSharp.Common;
using RazorSharp.Native.Enums;

#endregion

// ReSharper disable UnassignedReadonlyField

namespace RazorSharp.Native.Structures
{

	/// <summary>
	///     Contains information about a range of pages in the virtual address space of a process.
	///     The <see cref="Kernel32.VirtualQuery(IntPtr)" />,
	///     <see cref="Kernel32.VirtualQuery(IntPtr, ref MemoryBasicInformation, uint)" /> and VirtualQueryEx functions use
	///     this structure.
	///
	///     <a href="https://docs.microsoft.com/en-us/windows/desktop/api/winnt/ns-winnt-_memory_basic_information">
	///         Doc
	///     </a>
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct MemoryBasicInformation
	{
		/// <summary>
		///     A pointer to the base address of the region of pages.
		/// </summary>
		public readonly IntPtr BaseAddress;

		/// <summary>
		///     A pointer to the base address of a range of pages allocated by the <see cref="Kernel32.VirtualAlloc" /> function.
		///     The page pointed to by the <see cref="BaseAddress" /> member is contained within this allocation range.
		/// </summary>
		public readonly IntPtr AllocationBase;

		/// <summary>
		///     The memory protection option when the region was initially allocated. This member can be one of the memory
		///     protection constants or 0 if the caller does not have access.
		/// </summary>
		public readonly MemoryProtection AllocationProtect;

		/// <summary>
		///     The size of the region beginning at the base address in which all pages have identical attributes, in bytes.
		/// </summary>
		public readonly IntPtr RegionSize;

		/// <summary>
		///     The state of the pages in the region.
		/// </summary>
		public readonly MemState State;

		/// <summary>
		///     The access protection of the pages in the region. This member is one of the values listed for the
		///     <see cref="AllocationProtect" /> member.
		/// </summary>
		public readonly MemoryProtection Protect;

		/// <summary>
		///     The type of pages in the region.
		/// </summary>
		public readonly MemType Type;

		public override string ToString()
		{
			ConsoleTable table = new ConsoleTable("Field", "Value");
			table.AddRow("Base address", Hex.ToHex(BaseAddress));
			table.AddRow("Allocation base", Hex.ToHex(AllocationBase));
			table.AddRow("Allocation protect", AllocationProtect);
			table.AddRow("Region size", RegionSize);
			table.AddRow("State", State);
			table.AddRow("Protect", Protect);
			table.AddRow("Type", Type);
			return table.ToMarkDownString();
		}
	}

}