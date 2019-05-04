#region

using System;
using System.Diagnostics;
using RazorSharp.CoreClr.Structures;

#endregion

namespace RazorSharp.Memory.Extern.Symbols.Attributes
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
	///         will be set (<see cref="Functions.SetStableEntryPoint" />) to the address of the symbol's RVA +
	/// the corresponding <see cref="ProcessModule"/>'s <see cref="ProcessModule.BaseAddress"/>.
	///     </para>
	///     <para>This allows the calling of non-exported DLL functions.</para>
	/// <seealso cref="Symload"/>
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class SymcallAttribute : SymImportAttribute
	{
		public SymcallAttribute() : base() { }

		public SymcallAttribute(SymImportOptions options) : base(options) { }

		public SymcallAttribute(string symbol, SymImportOptions options = SymImportOptions.None) 
			: base(symbol, options) { }
	}
}