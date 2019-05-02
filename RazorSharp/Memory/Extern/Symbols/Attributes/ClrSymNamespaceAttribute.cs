#region

using System;
using RazorSharp.CoreClr;

#endregion

namespace RazorSharp.Memory.Extern.Symbols.Attributes
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class ClrSymNamespaceAttribute : SymNamespaceAttribute
	{
		public ClrSymNamespaceAttribute(string nameSpace)
			: base(Clr.ClrPdb.FullName, Clr.CLR_DLL_SHORT, nameSpace) { }

		public ClrSymNamespaceAttribute() : this(null) { }
	}
}