using System;
using System.Diagnostics;
using RazorSharp.Core;
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
			if (!IsSetup) {
				IsSetup = true;
				Global.WriteLine("{0}::{1}", Id, nameof(Setup));
			}
		}

		public override void Close()
		{
			if (IsSetup) {
				IsSetup = false;
				base.Close();
			}
		}
	}
}