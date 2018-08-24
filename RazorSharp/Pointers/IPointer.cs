#region

using System;

#endregion

namespace RazorSharp.Pointers
{

	public interface IPointer<T>
	{
		/// <summary>
		///     Indexes <see cref="Address"/> as a reference.
		/// </summary>
		ref T this[int index] { get; }

		/// <summary>
		///     Dereferences the pointer as the specified type.
		/// </summary>
		T Value { get; set; }

		/// <summary>
		///     Returns the value as a reference, without copying.
		/// </summary>
		ref T Reference { get; }

		/// <summary>
		///     Address being pointed to.
		/// </summary>
		IntPtr Address { get; set; }

		/// <summary>
		///     Size of the type being pointed to.
		/// </summary>
		int ElementSize { get; }

		/// <summary>
		///     Whether <see cref="Address"/> being pointed to is null.
		/// </summary>
		bool IsNull { get; }

		/// <summary>
		///     Whether <see cref="Address"/> is aligned on the current <see cref="IntPtr.Size" /> boundary.
		/// </summary>
		bool IsAligned { get; }


		/// <summary>
		///     Converts <see cref="Address"/> to a 32-bit signed integer.
		/// </summary>
		/// <returns></returns>
		int ToInt32();

		/// <summary>
		///     Converts <see cref="Address"/> to a 64-bit signed integer.
		/// </summary>
		/// <returns></returns>
		long ToInt64();
	}

}