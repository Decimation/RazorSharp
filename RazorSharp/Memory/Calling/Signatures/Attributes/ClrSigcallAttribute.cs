#region

#endregion

using RazorSharp.CoreClr;

namespace RazorSharp.Memory.Calling.Signatures.Attributes
{
	/// <inheritdoc />
	/// <summary>
	///     <see cref="T:RazorSharp.Memory.Calling.Signatures.Attributes.SigcallAttribute" /> for module <see cref="Clr.CLR_DLL_SHORT" />
	/// </summary>
	public class ClrSigcallAttribute : SigcallAttribute
	{
		public ClrSigcallAttribute() : base(Clr.CLR_DLL_SHORT, null)
		{
			
		}

		public ClrSigcallAttribute(string signature) : base(Clr.CLR_DLL_SHORT, signature) { }
	}
}