using System;
using JetBrains.Annotations;

namespace RazorSharp.Memory.Extern.Symbols.Attributes
{
	[MeansImplicitUse]
	[AttributeUsage(AttributeTargets.Field)]
	public class SymFieldAttribute : SymImportAttribute
	{
		/// <summary>
		/// The <see cref="Type"/> to load this field as. If left unset, the field will be interpreted as
		/// the target field's type.
		/// <remarks>To use this, <see cref="FieldOptions"/> must be <see cref="SymFieldOptions.LoadAs"/>.</remarks>
		/// </summary>
		public Type LoadAs { get; set; }

		/// <summary>
		/// Specifies the size of memory to be copied into the field. If this is not specified, the base size of
		/// the field type will be used. (<see cref="Unsafe.BaseSizeOfData(Type)"/>)
		///
		/// <remarks>To use this, <see cref="FieldOptions"/> must be <see cref="SymFieldOptions.LoadDirect"/>.</remarks>
		/// 
		/// </summary>
		public int SizeConst { get; set; } = Constants.INVALID_VALUE;

		/// <summary>
		/// Specifies how the target field will be loaded.
		/// </summary>
		public SymFieldOptions FieldOptions { get; set; } = SymFieldOptions.LoadAs;

		public SymFieldAttribute() : base()
		{
			
		}

		public SymFieldAttribute(SymImportOptions options) : base(options)
		{
			
		}

		public SymFieldAttribute(string symbol, SymImportOptions options = SymImportOptions.None)
			: base(symbol, options) { }
	}
}