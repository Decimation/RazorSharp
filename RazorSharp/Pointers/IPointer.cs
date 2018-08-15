#region

using System;

#endregion

namespace RazorSharp.Pointers
{

	public interface IPointer<T>
	{
		/// <summary>
		/// Indexes the current address as a reference.
		/// </summary>
		ref T this[int index] { get; }

		/// <summary>
		/// Dereferences the pointer as the specified type.
		/// </summary>
		T Value { get; set; }

		/// <summary>
		/// Returns the value as a reference, without copying.
		/// </summary>
		ref T Reference { get; }

		/// <summary>
		/// Address being pointed to.
		/// </summary>
		IntPtr Address { get; set; }

		/// <summary>
		/// Size of the type being pointed to.
		/// </summary>
		int ElementSize { get; }

		/// <summary>
		/// Whether the current address being pointed to is null.
		/// </summary>
		bool IsNull { get; }

		/// <summary>
		/// Whether the current address is aligned on the current IntPtr.Size boundary.
		/// </summary>
		bool IsAligned { get; }


		/// <summary>
		/// Converts the current address to a 32-bit signed integer.
		/// </summary>
		/// <returns></returns>
		int ToInt32();

		/// <summary>
		/// Converts the current address to a 64-bit signed integer.
		/// </summary>
		/// <returns></returns>
		long ToInt64();
	}

}