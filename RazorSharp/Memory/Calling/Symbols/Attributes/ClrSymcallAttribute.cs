using RazorSharp.CoreClr;
using RazorSharp.Native;

namespace RazorSharp.Memory.Calling.Symbols.Attributes
{
	public class ClrSymcallAttribute : SymcallAttribute
	{
		public ClrSymcallAttribute()
		{
			Image = Clr.ClrPdb.FullName;
			Module = Clr.CLR_DLL_SHORT;
		}
	}
}