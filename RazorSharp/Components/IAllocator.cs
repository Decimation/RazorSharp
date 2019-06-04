#region

using RazorSharp.Import;
using RazorSharp.Memory;

#endregion

namespace RazorSharp.Components
{
	/// <summary>
	///     <list type="bullet">
	///         <listheader>Inheriting types:</listheader>
	///         <item>
	///             <description>
	///                 <see cref="Symload" />
	///             </description>
	///         </item>
	///         <item>
	///             <description>
	///                 <see cref="Mem" />
	///             </description>
	///         </item>
	///     </list>
	/// </summary>
	public interface IAllocator
	{
		int  AllocCount    { get; /* private set; */ }
		bool IsMemoryInUse { get; }
	}
}