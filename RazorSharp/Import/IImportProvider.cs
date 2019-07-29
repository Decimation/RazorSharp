using System;
using RazorSharp.Memory.Pointers;

namespace RazorSharp.Import
{
	/// <summary>
	/// Describes a type that provides methods for use in <see cref="ImportManager"/>.
	/// </summary>
	public interface IImportProvider
	{
		/// <summary>
		/// Retrieves the address of the specified member specified by <paramref name="id"/>.
		/// </summary>
		/// <param name="id">Any identifier for the imported member</param>
		/// <returns>Address of the imported member</returns>
		Pointer<byte> GetAddress(string id);

		/// <summary>
		/// Creates a <typeparamref name="TDelegate"/> from the imported member
		/// specified by <paramref name="id"/>.
		/// </summary>
		/// <param name="id">Any identifier for the imported function</param>
		/// <returns><see cref="Delegate"/> of type <typeparamref name="TDelegate"/></returns>
		TDelegate GetFunction<TDelegate>(string id) where TDelegate : Delegate;
	}
}