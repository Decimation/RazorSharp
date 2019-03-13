#region

using RazorSharp.Clr;

#endregion

namespace RazorSharp.Memory.Attributes
{
	/// <inheritdoc />
	/// <summary>
	///     <see cref="T:RazorSharp.Memory.Attributes.SigcallAttribute" /> for module <see cref="Clr.CLR_DLL" />
	/// </summary>
	public class ClrSigcallAttribute : SigcallAttribute
	{
		public ClrSigcallAttribute() : base(Clr.Clr.CLR_DLL, null)
		{
			IsInFunctionMap = true;
		}

		public ClrSigcallAttribute(string signature) : base(Clr.Clr.CLR_DLL, signature) { }
	}
}