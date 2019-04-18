#region

using System;
using RazorSharp.CoreClr;

#endregion

namespace RazorSharp.Memory.Extern.Symbols.Attributes
{
	
	[AttributeUsage(AttributeTargets.Method)]
	public class ClrSymcallAttribute : SymcallAttribute
	{
		public ClrSymcallAttribute()
		{
			Image  = Clr.ClrPdb.FullName;
			Module = Clr.CLR_DLL_SHORT;
		}
	}
}