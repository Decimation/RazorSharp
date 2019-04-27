#region

using System;
using RazorSharp.CoreClr.Structures;

#endregion

namespace RazorSharp.Memory.Extern.Signatures.Attributes
{
	/// <inheritdoc />
	/// <summary>
	///     <para>
	///         Indicates that the attributed function is implemented via signature scanning (using
	///         <see cref="T:RazorSharp.Memory.SigScanner" /> internally).
	///     </para>
	///     <para>
	///         The annotated method's entry point (<see cref="MethodDesc.Function" />)
	///         will be set (<see cref="Functions.SetStableEntryPoint" />) to the address of the matched signature found by
	///         <see cref="MemScanner" />.
	///     </para>
	///     <para>This allows the calling of non-exported DLL functions, so long as the function signature matches.</para>
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class SigcallAttribute : Attribute
	{
		public SigcallAttribute() { }

		public SigcallAttribute(string module, string signature)
		{
			Module    = module;
			Signature = signature;
		}

		/// <summary>
		///     Module (DLL) containing <see cref="Signature" />
		/// </summary>
		public string Module { get; set; }

		/// <summary>
		///     Relative to the module's <see cref="MemScanner.m_lo" />
		/// </summary>
		public long OffsetGuess { get; set; }

		/// <summary>
		///     Unique byte-sequence-string signature of the function
		/// </summary>
		public string Signature { get; set; }
	}
}