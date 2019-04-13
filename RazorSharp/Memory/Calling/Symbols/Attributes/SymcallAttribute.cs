#region

using System;
using System.Diagnostics;

#endregion

namespace RazorSharp.Memory.Calling.Symbols.Attributes
{
	[AttributeUsage(AttributeTargets.Method)]
	public class SymcallAttribute : Attribute
	{
		// todo: this configuration is a bit confusing

		/// <summary>
		///     Name of the method
		/// </summary>
		public string Symbol { get; set; }

		/// <summary>
		///     Debugging symbol file (PDB, etc)
		/// </summary>
		public string Image { get; set; }

		/// <summary>
		///     <see cref="ProcessModule" /> from which to calculate the function address
		/// </summary>
		public string Module { get; set; }

		/// <summary>
		///     Whether <see cref="Symbol" /> is the fully qualified name
		/// </summary>
		public bool FullyQualified { get; set; }

		/// <summary>
		///     Whether to use the decorated method's name as the symbol.
		/// </summary>
		public bool UseMethodNameOnly { get; set; }

		/// <summary>
		///     Whether to ignore <see cref="SymNamespaceAttribute.Namespace" /> if the enclosing type
		///     specifies a namespace.
		/// </summary>
		public bool IgnoreNamespace { get; set; }
	}
}