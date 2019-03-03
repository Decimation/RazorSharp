#region

using System;
using RazorSharp.CLR;

#endregion

namespace RazorSharp.Memory
{
	/// <inheritdoc />
	/// <summary>
	///     <para>
	///         Indicates that the attributed function is exposed via signature scanning (using
	///         <see cref="T:RazorSharp.Memory.SigScanner" /> internally).
	///     </para>
	///     <para>
	///         The annotated method's entry point (<see cref="RazorSharp.CLR.Structures.MethodDesc.Function" />)
	///         will be set (<see cref="ClrFunctions.SetStableEntryPoint" />) to the address of the matched signature found by
	///         <see cref="SigScanner" />.
	///     </para>
	///     <para>This allows the calling of non-exported DLL functions, so long as the function signature matches.</para>
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class SigcallAttribute : Attribute
	{
		/// <summary>
		/// </summary>
		public bool IsInFunctionMap;

		/// <summary>
		///     Module (DLL) containing <see cref="Signature" />
		/// </summary>
		public string Module;

		/// <summary>
		///     Relative to the module's <see cref="SigScanner.BaseAddress" />
		/// </summary>
		public long OffsetGuess;

		/// <summary>
		///     Unique byte-sequence-string signature of the function
		/// </summary>
		public string Signature;

		public SigcallAttribute() { }

		public SigcallAttribute(string module, string signature)
		{
			Module          = module;
			Signature       = signature;
			IsInFunctionMap = false;
		}
	}
}