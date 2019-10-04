using System;

namespace RazorSharp.Memory.Enums
{
	/// <summary>
	/// Specifies how a value will be converted
	/// </summary>
	public enum ConversionType
	{
		/// <summary>
		///     Reinterprets a value as a value of the specified conversion type
		/// </summary>
		Reinterpret,

		/// <summary>
		/// <see cref="Convert.ChangeType(object,System.Type)"/>
		/// </summary>
		Normal,

		/// <summary>
		/// <see cref="Unsafe.As{T,T}"/>
		/// </summary>
		Proxy
	}
}