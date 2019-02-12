#region

#region

using System;

#endregion

// ReSharper disable UnusedMemberInSuper.Global

#endregion

namespace RazorSharp.Pointers
{
	/// <inheritdoc />
	/// <summary>
	///     <para>
	///         The interface for <see cref="Pointer{T}" />. This interface is kept <c>internal</c> because we don't
	///         want to cause accidental boxing allocations.
	///     </para>
	///     <para>
	///         All <see cref="IPointer{T}" /> types should be <c>struct</c> types
	///         so they can equal the size and layout of a native pointer in memory.
	///     </para>
	///     <para>Therefore, this interface just serves as a static contract and shouldn't be used in runtime.</para>
	/// </summary>
	/// <typeparam name="T">Element type</typeparam>
	internal unsafe interface IPointer<T> : IFormattable
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
		///     Returns the value as a reference.
		/// </summary>
		ref T Reference { get; }

		/// <summary>
		///     Address being pointed to.
		/// </summary>
		IntPtr Address { get; set; }

		/// <summary>
		///     Size of type <typeparamref name="T" />.
		/// </summary>
		int ElementSize { get; }

		/// <summary>
		///     Whether <see cref="Address" /> is <c>null</c> (<see cref="IntPtr.Zero" />).
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

		/// <summary>
		///     Converts <see cref="Address" /> to a 32-bit unsigned integer.
		/// </summary>
		/// <returns></returns>
		uint ToUInt32();

		/// <summary>
		///     Converts <see cref="Address" /> to a 64-bit unsigned integer.
		/// </summary>
		/// <returns></returns>
		ulong ToUInt64();

		/// <summary>
		///     Returns <see cref="Address" /> as a pointer.
		/// </summary>
		/// <returns></returns>
		void* ToPointer();

		/// <summary>
		///     Reads a value of type <typeparamref name="T" /> from <see cref="Address" />
		/// </summary>
		/// <param name="elemOffset">Element offset (of type <typeparamref name="T" />)</param>
		/// <returns>The value read from the offset <see cref="Address" /></returns>
		T Read(int elemOffset = 0);

		/// <summary>
		///     Writes a value of type <typeparamref name="T" /> to <see cref="Address" />
		/// </summary>
		/// <param name="t">Value to write</param>
		/// <param name="elemOffset">Element offset (of type <typeparamref name="T" />)</param>
		void Write(T t, int elemOffset = 0);

		/// <summary>
		///     Reinterprets <see cref="Address" /> as a reference to a value of type <typeparamref name="T" />
		/// </summary>
		/// <param name="elemOffset">Element offset (of type <typeparamref name="T" />)</param>
		/// <returns>A reference to a value of type <typeparamref name="T" /></returns>
		ref T AsRef(int elemOffset = 0);

		/// <summary>
		///     Checks to see if <see cref="other" /> is equal to the current instance.
		/// </summary>
		/// <param name="other">Other <see cref="IPointer{T}" /></param>
		/// <returns></returns>
		bool Equals(Pointer<T> other);
	}
}