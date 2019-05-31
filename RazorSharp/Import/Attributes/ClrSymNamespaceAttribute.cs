using System;
using JetBrains.Annotations;
using RazorSharp.CoreClr;

namespace RazorSharp.Import.Attributes
{
	[MeansImplicitUse]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class ClrSymNamespaceAttribute : SymNamespaceAttribute
	{
		public ClrSymNamespaceAttribute(string nameSpace) 
			: base(Clr.Value.ClrPdb.FullName, Clr.CLR_DLL_SHORT, nameSpace) { }

		public ClrSymNamespaceAttribute() : this(null) { }
	}
}