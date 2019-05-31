using RazorSharp.CoreClr;
using RazorSharp.Native.Symbols;
using SimpleSharp.Diagnostics;

namespace RazorSharp
{
	/// <summary>
	///     <list type="bullet">
	///         <listheader>Inheriting types:</listheader>
	///         <item>
	///             <description>
	///                 <see cref="Global" />
	///             </description>
	///         </item>
	///         <item>
	///             <description>
	///                 <see cref="Clr" />
	///             </description>
	///         </item>
	///         <item>
	///             <description>
	///                 <see cref="ModuleInitializer" />
	///             </description>
	///         </item>
	///         <item>
	///             <description>
	///                 <see cref="SymbolManager" />
	///             </description>
	///         </item>
	///     </list>
	/// </summary>
	public abstract class Releasable
	{
		public bool IsSetup { get; protected set;  }
		
		public virtual void Setup()
		{
			IsSetup = true;
		}

		public virtual void Close()
		{
			Conditions.Require(IsSetup, nameof(IsSetup));
			IsSetup = false;
		}
	}
}