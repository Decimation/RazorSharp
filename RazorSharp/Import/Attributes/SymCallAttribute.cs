#region

using System;
using System.Diagnostics;
using JetBrains.Annotations;
using RazorSharp.CoreClr.Structures;
using RazorSharp.Memory;

#endregion

namespace RazorSharp.Import.Attributes
{
	/// <inheritdoc />
	/// <summary>
	///     <para>
	///         Indicates that the attributed function is implemented by matching the image of a symbol with its
	/// 		corresponding <see cref="ProcessModule"/>. The symbol's RVA is added to the module's base address to
	/// retrieve the symbol's value (entry point).
	///     </para>
	///     <para>
	///         The annotated method's entry point (<see cref="MethodDesc.Function" />)
	///         will be set (<see cref="Functions.SetEntryPoint" />) to the address of the symbol's RVA +
	/// the corresponding <see cref="ProcessModule"/>'s <see cref="ProcessModule.BaseAddress"/>.
	///     </para>
	///     <para>This allows the calling of non-exported DLL functions.</para>
	/// <seealso cref="Symload"/>
	/// </summary>
	[MeansImplicitUse]
	[AttributeUsage(AttributeTargets.Method)]
	public class SymCallAttribute : SymImportAttribute
	{
		public SymCallAttribute() : base() { }

		public SymCallAttribute(SymImportOptions options) : base(options) { }

		public SymCallAttribute(string symbol, SymImportOptions options = SymImportOptions.None) 
			: base(symbol, options) { }
	}
}