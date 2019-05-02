#region

using System;

#endregion

namespace RazorSharp.Memory.Extern.Symbols.Attributes
{
	[AttributeUsage(AttributeTargets.Field)]
	public class SymFieldAttribute : SymImportAttribute
	{
		/// <summary>
		///     The <see cref="Type" /> to load this field as. If left unset, the field will be interpreted as
		///     the target field's type.
		/// </summary>
		public Type LoadAs { get; set; }

		/// <summary>
		///     Whether this should be interpreted as a global variable.
		///     <remarks>
		///         (Shortcut for <see cref="SymImportAttribute.IgnoreNamespace" />,
		///         <see cref="SymImportAttribute.UseMemberNameOnly" /> and <see cref="SymImportAttribute.FullyQualified" />)
		///     </remarks>
		/// </summary>
		public bool Global {
			get => IgnoreNamespace && FullyQualified && UseMemberNameOnly;
			set {
				IgnoreNamespace   = value;
				FullyQualified    = value;
				UseMemberNameOnly = value;
			}
		}
	}
}