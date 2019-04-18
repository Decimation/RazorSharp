#region

using System;

#endregion

namespace RazorSharp.Memory.Extern.Symbols.Attributes
{
	/// <summary>
	///     All member <see cref="SymImportAttribute" /> <see cref="SymImportAttribute.Symbol" />s in the annotated class
	///     or struct will be prefixed with <see cref="Namespace" /> if the attribute has not set <seealso cref="SymImportAttribute.IgnoreNamespace"/>
	/// to <c>true</c>
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class SymNamespaceAttribute : Attribute
	{
		public SymNamespaceAttribute(string nameSpace)
		{
			Namespace = nameSpace;
		}

		public string Namespace { get; set; }
	}
}