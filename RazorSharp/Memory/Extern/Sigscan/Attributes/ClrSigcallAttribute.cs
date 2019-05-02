#region

using System;
using RazorSharp.CoreClr;

#endregion

namespace RazorSharp.Memory.Extern.Sigscan.Attributes
{
	/// <inheritdoc />
	/// <summary>
	///     <see cref="T:RazorSharp.Memory.Extern.Sigscan.Attributes.SigcallAttribute" /> for module
	///     <see cref="Clr.CLR_DLL_SHORT" />
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class ClrSigcallAttribute : SigcallAttribute
	{
		public ClrSigcallAttribute() : base(Clr.CLR_DLL_SHORT, null) { }

		public ClrSigcallAttribute(string signature) : base(Clr.CLR_DLL_SHORT, signature) { }
	}
}