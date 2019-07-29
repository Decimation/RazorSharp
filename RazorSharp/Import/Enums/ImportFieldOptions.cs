using RazorSharp.Import.Attributes;

namespace RazorSharp.Import.Enums
{
	/// <summary>
	/// Specifies how the field will be loaded.
	/// </summary>
	public enum ImportFieldOptions
	{
		/// <summary>
		/// Copy the value directly into the field from a byte array. Use <see cref="ImportFieldAttribute.SizeConst"/> to
		/// specify the size of the value. If the value isn't set, the size of the target field will be used.
		///
		/// <remarks>
		/// If the target field is a reference type, the memory is copied into the data (fields) of the object, not the
		/// pointer itself.
		/// </remarks>
		/// </summary>
		CopyIn,
		
		/// <summary>
		/// Loads the value as the type specified by <see cref="ImportFieldAttribute.LoadAs"/>
		/// (or the field type if the type isn't specified)
		/// </summary>
		Proxy,
		
		Fast
	}
}