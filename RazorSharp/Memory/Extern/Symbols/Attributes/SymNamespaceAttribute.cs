#region

using System;
using System.Diagnostics;

#endregion

namespace RazorSharp.Memory.Extern.Symbols.Attributes
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class SymNamespaceAttribute : Attribute
	{
		public SymNamespaceAttribute(string img, string module, string nameSpace = null)
		{
			Image     = img;
			Module    = module;
			Namespace = nameSpace;
		}

		/// <summary>
		///     All member <see cref="SymImportAttribute" /> <see cref="SymImportAttribute.Symbol" />s in the
		///     annotated class or struct will be prefixed with <see cref="Namespace" /> if the attribute has not
		///     set <seealso cref="SymImportAttribute.IgnoreNamespace" /> to <c>true</c>
		/// </summary>
		public string Namespace { get; set; }

		/// <summary>
		///     Debugging symbol file (PDB, etc)
		/// </summary>
		public string Image { get; set; }

		/// <summary>
		///     <see cref="ProcessModule" /> from which to calculate the value address
		/// </summary>
		public string Module { get; set; }
	}
}