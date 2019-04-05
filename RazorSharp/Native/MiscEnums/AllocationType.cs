#region

using System;

#endregion

namespace RazorSharp.Native.Enums
{
	/// <summary>
	///     <para>
	///         <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/aa366887(v=vs.85).aspx"> Doc </a>
	///     </para>
	/// </summary>
	[Flags]
	public enum AllocationType : uint
	{
		/// <summary>
		///     Allocates memory charges (from the overall size of memory and the paging files on disk) for the specified reserved
		///     memory pages. The function also guarantees that when the caller later initially accesses the memory, the contents
		///     will be zero. Actual physical pages are not allocated unless/until the virtual addresses are actually accessed.
		/// </summary>
		Commit = 0x1000,

		/// <summary>
		///     Reserves a range of the process's virtual address space without allocating any actual physical storage in memory or
		///     in the paging file on disk.
		/// </summary>
		Reserve = 0x2000,

		/// <summary>
		///     Indicates that data in the memory range specified by lpAddress and dwSize is no longer of interest. The pages
		///     should not be read from or written to the paging file. However, the memory block will be used again later, so it
		///     should not be decommitted. This value cannot be used with any other value.
		/// </summary>
		Decommit = 0x4000,

		Release = 0x8000,

		Reset = 0x80000,

		Physical = 0x400000,

		TopDown = 0x100000,

		WriteWatch = 0x200000,

		LargePages = 0x20000000
	}
}