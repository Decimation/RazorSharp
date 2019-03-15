#region

using RazorSharp.Clr;

#endregion

namespace RazorSharp.Memory.Calling.Signatures.Attributes
{
	/// <inheritdoc />
	/// <summary>
	///     <see cref="T:RazorSharp.Memory.Calling.Signatures.Attributes.SigcallAttribute" /> for module <see cref="Clr.CLR_DLL" />
	/// </summary>
	public class ClrSigcallAttribute : SigcallAttribute
	{
		public ClrSigcallAttribute() : base(Clr.Clr.CLR_DLL, null)
		{
			
		}

		public ClrSigcallAttribute(string signature) : base(Clr.Clr.CLR_DLL, signature) { }
	}
}