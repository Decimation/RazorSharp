#region

using RazorSharp.Clr;

#endregion

namespace RazorSharp.Memory
{
	/// <inheritdoc />
	/// <summary>
	///     <see cref="T:RazorSharp.Memory.SigcallAttribute" /> for module <see cref="ClrFunctions.CLR_DLL" />
	/// </summary>
	public class ClrSigcallAttribute : SigcallAttribute
	{
		public ClrSigcallAttribute() : base(ClrFunctions.CLR_DLL, null)
		{
			IsInFunctionMap = true;
		}

		public ClrSigcallAttribute(string signature) : base(ClrFunctions.CLR_DLL, signature) { }
	}
}