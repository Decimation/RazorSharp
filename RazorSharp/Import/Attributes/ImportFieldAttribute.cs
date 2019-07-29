using System;
using JetBrains.Annotations;
using RazorSharp.CoreClr;
using RazorSharp.Import.Enums;
using RazorSharp.Memory;

namespace RazorSharp.Import.Attributes
{
	[MeansImplicitUse]
	[AttributeUsage(FIELD_TARGETS)]
	public class ImportFieldAttribute : ImportAttribute
	{
		internal const AttributeTargets FIELD_TARGETS = AttributeTargets.Field;
		
		/// <summary>
		/// The <see cref="Type"/> to load this field as. If left unset, the field will be interpreted as
		/// the target field's type.
		/// <remarks>To use this, <see cref="FieldOptions"/> must be <see cref="ImportFieldOptions.Proxy"/>.</remarks>
		/// </summary>
		public Type LoadAs { get; set; }

		/// <summary>
		/// Specifies the size of memory to be copied into the field. If this is not specified, the base size of
		/// the field type will be used. (<see cref="Unsafe.BaseSizeOfData"/>)
		///
		/// <remarks>To use this, <see cref="FieldOptions"/> must be <see cref="ImportFieldOptions.CopyIn"/>.</remarks>
		/// 
		/// </summary>
		public int SizeConst { get; set; } = Constants.INVALID_VALUE;

		/// <summary>
		/// Specifies how the target field will be loaded.
		/// </summary>
		public ImportFieldOptions FieldOptions { get; set; } = ImportFieldOptions.Proxy;

		public ImportFieldAttribute() { }

		public ImportFieldAttribute(IdentifierOptions options, ImportFieldOptions loadOptions) : this(options)
		{
			FieldOptions = loadOptions;
		}

		public ImportFieldAttribute(IdentifierOptions options) : base(options) { }

		public ImportFieldAttribute(string id, IdentifierOptions options = IdentifierOptions.None) 
			: base(id, options) { }
	}
}