#region

using System;

#endregion

namespace RazorSharp.Pointers
{

	public unsafe interface IPointer<T> : IFormattable
	{
		/// <summary>
		///     Indexes <see cref="Address" /> as a reference.
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
		///     Whether <see cref="Address" /> being pointed to is null.
		/// </summary>
		bool IsNull { get; }

		/// <summary>
		///     Whether <see cref="Address" /> is aligned on the current <see cref="IntPtr.Size" /> boundary.
		/// </summary>
		bool IsAligned { get; }


		/// <summary>
		///     Converts <see cref="Address" /> to a 32-bit signed integer.
		/// </summary>
		/// <returns></returns>
		int ToInt32();

		/// <summary>
		///     Converts <see cref="Address" /> to a 64-bit signed integer.
		/// </summary>
		/// <returns></returns>
		long ToInt64();

		void* ToPointer();

		/// <summary>
		///     Read from <see cref="Address" />
		/// </summary>
		/// <param name="elemOffset">Element offset</param>
		/// <typeparam name="TType">Type to read</typeparam>
		/// <returns>The value read from the offset <see cref="Address" /></returns>
		TType Read<TType>(int elemOffset = 0);

		/// <summary>
		///     Write to <see cref="Address" />
		/// </summary>
		/// <param name="t">Value to write</param>
		/// <param name="elemOffset">Element offset</param>
		/// <typeparam name="TType">Type to write</typeparam>
		void Write<TType>(TType t, int elemOffset = 0);


		ref TType AsRef<TType>(int elemOffset = 0);

		bool Equals(IPointer<T> other);


		Pointer<TNew> Reinterpret<TNew>();

	}

}