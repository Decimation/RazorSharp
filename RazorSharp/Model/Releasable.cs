using System;
using System.Diagnostics;
using RazorSharp.Import;
using SimpleSharp.Diagnostics;

namespace RazorSharp.Model
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
	///                 <see cref="ModuleInitializer" /> (implicit)
	///             </description>
	///         </item>
	///         <item>
	///             <description>
	///                 <see cref="SymbolManager" />
	///             </description>
	///         </item>
	/// <item>
	///             <description>
	///                 <see cref="ImportManager" />
	///             </description>
	///         </item>
	///     </list>
	/// </summary>
	public abstract class Releasable : Closable
	{
		public bool IsSetup { get; protected set; }
		
		public Releasable() { }

		public virtual void Setup()
		{
			IsSetup = true;
		}

		public override void Close()
		{
			Conditions.Require(IsSetup, nameof(IsSetup));
			IsSetup = false;
			base.Close();
		}
	}
}