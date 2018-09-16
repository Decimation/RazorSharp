#region

using System;
using System.Runtime.CompilerServices;

#endregion

namespace RazorSharp.Pointers
{

	/// <summary>
	///     Provides utilities for pointer arithmetic
	/// </summary>
	public static unsafe class PointerUtils
	{
		/// <summary>
		///     Subtracts <paramref name="right" /> bytes from <paramref name="left" />'s <see cref="Pointer{T}.Address" />
		/// </summary>
		/// <param name="left">Left <see cref="Pointer{T}" /></param>
		/// <param name="right">Number of bytes to subtract</param>
		/// <returns><paramref name="left" /> with <paramref name="right" /> bytes subtracted</returns>
		public static Pointer<byte> Subtract(Pointer<byte> left, long right)
		{
			long val = left.ToInt64() - right;
			return new Pointer<byte>(val);
		}

		/// <summary>
		///     Adds <paramref name="right" /> bytes to <paramref name="left" />'s <see cref="Pointer{T}.Address" />
		/// </summary>
		/// <param name="left">Left <see cref="Pointer{T}" /></param>
		/// <param name="right">Number of bytes to add</param>
		/// <returns><paramref name="left" /> with <paramref name="right" /> bytes added</returns>
		public static Pointer<byte> Add(Pointer<byte> left, long right)
		{
			long val = left.ToInt64() + right;
			return new Pointer<byte>(val);
		}

		/// <summary>
		///     Subtracts <paramref name="right" />'s <see cref="Pointer{T}.Address" /> from <paramref name="left" />'s
		///     <see cref="Pointer{T}.Address" />
		/// </summary>
		/// <param name="left">Left <see cref="Pointer{T}" /></param>
		/// <param name="right">Right <see cref="Pointer{T}" /></param>
		/// <returns><paramref name="left" /> with <paramref name="right" />'s <see cref="Pointer{T}.Address" /> subtracted</returns>
		public static Pointer<byte> Subtract(Pointer<byte> left, Pointer<byte> right)
		{
			return Subtract(left, right.ToInt64());
		}

		/// <summary>
		///     Adds <paramref name="right" />'s <see cref="Pointer{T}.Address" /> address to <paramref name="left" />'s
		///     <see cref="Pointer{T}.Address" />
		/// </summary>
		/// <param name="left">Left <see cref="Pointer{T}" /></param>
		/// <param name="right">Right <see cref="Pointer{T}" /></param>
		/// <returns><paramref name="left" /> with <paramref name="right" />'s <see cref="Pointer{T}.Address" /> added</returns>
		public static Pointer<byte> Add(Pointer<byte> left, Pointer<byte> right)
		{
			return Add(left, right.ToInt64());
		}

		/// <summary>
		///     Offsets a pointer by <paramref name="elemCnt" /> elements.
		/// </summary>
		/// <param name="p"><see cref="IntPtr" /> pointer</param>
		/// <param name="elemCnt">Elements to offset by</param>
		/// <typeparam name="T">Element type</typeparam>
		/// <returns>
		///     <paramref name="p" /> <c>+</c> <c>(</c><paramref name="elemCnt" /> <c>*</c> <see cref="Unsafe.SizeOf{T}" /><c>)</c>
		/// </returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IntPtr Offset<T>(IntPtr p, int elemCnt)
		{
			int size = Unsafe.SizeOf<T>();
			size *= elemCnt;
			return p + size;
		}

		/// <summary>
		///     Offsets a pointer by <paramref name="elemCnt" /> elements.
		/// </summary>
		/// <param name="p"><c>void*</c> pointer</param>
		/// <param name="elemCnt">Elements to offset by</param>
		/// <typeparam name="T">Element type</typeparam>
		/// <returns>
		///     <paramref name="p" /> <c>+</c> <c>(</c><paramref name="elemCnt" /> <c>*</c> <see cref="Unsafe.SizeOf{T}" /><c>)</c>
		/// </returns>
		public static IntPtr Offset<T>(void* p, int elemCnt)
		{
			return Offset<T>((IntPtr) p, elemCnt);
		}

		/// <summary>
		///     Returns the element index of a pointer relative to <paramref name="orig" />
		/// </summary>
		/// <param name="orig">Origin pointer (low address)</param>
		/// <param name="current">Current pointer (high address)</param>
		/// <typeparam name="TElement">Element type</typeparam>
		/// <returns>The index</returns>
		public static int OffsetIndex<TElement>(IntPtr orig, IntPtr current)
		{
			long byteDelta = current.ToInt64() - orig.ToInt64();
			return (int) byteDelta / Unsafe.SizeOf<TElement>();
		}
	}

}