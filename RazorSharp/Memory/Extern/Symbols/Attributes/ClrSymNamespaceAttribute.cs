using System;
using JetBrains.Annotations;
using RazorSharp.CoreClr;

namespace RazorSharp.Memory.Extern.Symbols.Attributes
{
	[MeansImplicitUse]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class ClrSymNamespaceAttribute : SymNamespaceAttribute
	{
		public ClrSymNamespaceAttribute(string nameSpace) 
			: base(Clr.ClrPdb.FullName, Clr.CLR_DLL_SHORT, nameSpace) { }

		public ClrSymNamespaceAttribute() : this(null) { }
	}
}