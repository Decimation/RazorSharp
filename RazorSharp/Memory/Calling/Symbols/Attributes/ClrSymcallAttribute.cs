using RazorSharp.Native;

namespace RazorSharp.Memory.Calling.Symbols.Attributes
{
	public class ClrSymcallAttribute : SymcallAttribute
	{
		public ClrSymcallAttribute()
		{
			Image = Symbolism.CLR_PDB;
			Module = Clr.Clr.CLR_DLL;
		}
	}
}