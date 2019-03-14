namespace RazorSharp.Memory
{
	//todo
	/// <summary>
	///     http://www.hexacorn.com/blog/2016/12/15/pe-section-names-re-visited/
	/// </summary>
	public enum SegmentType
	{
		/// <summary>
		///     <c>const</c> data; readonly of <see cref="DATA" />
		/// </summary>
		RDATA,

		/// <summary>
		///     Import directory; designates the imported and exported functions
		/// </summary>
		IDATA,

		/// <summary>
		///     Initialized data
		/// </summary>
		DATA,

		/// <summary>
		///     Exception information
		/// </summary>
		PDATA,

		/// <summary>
		///     Uninitialized data
		/// </summary>
		BSS,

		/// <summary>
		///     Resource directory
		/// </summary>
		RSRC,

		/// <summary>
		///     Image relocations
		/// </summary>
		RELOC,

		/// <summary>
		///     Executable code. Also known as the <c>code</c> segment.
		/// </summary>
		TEXT,

		/// <summary>
		///     Delay import section
		/// </summary>
		DIDAT
	}
}