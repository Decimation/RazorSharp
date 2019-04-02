#region

using System;

#endregion

namespace RazorSharp.Memory.Calling.Symbols.Attributes
{
	/// <summary>
	///     All member <see cref="SymcallAttribute" /> <see cref="SymcallAttribute.Symbol" />s in the annotated class
	///     or struct will be prefixed with <see cref="Namespace" />.
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