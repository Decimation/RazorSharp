using System;

namespace RazorSharp.Native.Enums
{
	/// <summary>
	///     <a href="https://docs.microsoft.com/en-us/windows/desktop/api/winnt/ns-winnt-_memory_basic_information">
	///         Doc
	///     </a>
	/// </summary>
	[Flags]
	public enum MemType
	{
		/// <summary>
		///     Indicates that the memory pages within the region are mapped into the view of an image section.
		/// </summary>
		Image = 0x1000000,

		/// <summary>
		///     Indicates that the memory pages within the region are mapped into the view of a section.
		/// </summary>
		Mapped = 0x40000,

		/// <summary>
		///     Indicates that the memory pages within the region are private (that is, not shared by other processes).
		/// </summary>
		Private = 0x20000
	}
}