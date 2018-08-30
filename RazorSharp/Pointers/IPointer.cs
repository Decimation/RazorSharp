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
		///     Whether <see cref="Address" /> is null.
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
		///     Returns <see cref="Address" /> as a pointer.
		/// </summary>
		/// <returns></returns>
		void* ToPointer();

		/// <summary>
		///     Reads a value of type <typeparamref name="TType" /> from <see cref="Address" />
		/// </summary>
		/// <param name="elemOffset">Element offset</param>
		/// <typeparam name="TType">Type to read</typeparam>
		/// <returns>The value read from the offset <see cref="Address" /></returns>
		TType Read<TType>(int elemOffset = 0);

		/// <summary>
		///     Writes a value of type <typeparamref name="TType" /> to <see cref="Address" />
		/// </summary>
		/// <param name="t">Value to write</param>
		/// <param name="elemOffset">Element offset</param>
		/// <typeparam name="TType">Type to write</typeparam>
		void Write<TType>(TType t, int elemOffset = 0);


		/// <summary>
		///     Reinterprets <see cref="Address" /> as a reference to a value of type <typeparamref name="TType" />
		/// </summary>
		/// <param name="elemOffset">Element offset</param>
		/// <typeparam name="TType">Type to reference</typeparam>
		/// <returns>A reference to a value of type <typeparamref name="TType" /></returns>
		ref TType AsRef<TType>(int elemOffset = 0);

		/// <summary>
		///     Checks to see if <see cref="other" /> is equal to the current instance.
		/// </summary>
		/// <param name="other">Other <see cref="IPointer{T}" /></param>
		/// <returns></returns>
		bool Equals(IPointer<T> other);

		/// <summary>
		///     Creates a new <see cref="Pointer{T}" /> of type <typeparamref name="TNew" />, pointing to <see cref="Address" />
		/// </summary>
		/// <typeparam name="TNew">Type to point to</typeparam>
		/// <returns>A new <see cref="Pointer{T}" /> of type <typeparamref name="TNew" /></returns>
		Pointer<TNew> Reinterpret<TNew>();

		/// <summary>
		///     Increment the <see cref="Address" /> by the specified number of bytes
		/// </summary>
		/// <param name="bytes">Number of bytes to add</param>
		void Add(int bytes);


		/// <summary>
		///     Decrement <see cref="Address" /> by the specified number of bytes
		/// </summary>
		/// <param name="bytes">Number of bytes to subtract</param>
		void Subtract(int bytes);


		/// <summary>
		///     Increment the <see cref="Address" /> by the specified number of elements
		/// </summary>
		/// <param name="elemCnt">Number of elements</param>
		void Increment(int elemCnt = 1);

		/// <summary>
		///     Decrement the <see cref="Address" /> by the specified number of elements
		/// </summary>
		/// <param name="elemCnt">Number of elements</param>
		void Decrement(int elemCnt = 1);


	}

}