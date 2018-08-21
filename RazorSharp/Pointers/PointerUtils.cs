#region

using System;
using System.Runtime.CompilerServices;

#endregion

namespace RazorSharp.Pointers
{

	public static unsafe class PointerUtils
	{
		public static IntPtr Subtract(IntPtr a, IntPtr b)
		{
			long al = a.ToInt64();
			long bl = b.ToInt64();
			long c  = al - bl;
			return (IntPtr) c;
		}

		public static IntPtr Subtract(void* v, int bytes)
		{
			return (IntPtr) ((long) v - bytes);
		}

		public static IntPtr Add(void* v, int bytes)
		{
			return (IntPtr) ((long) v + bytes);
		}

		public static IntPtr Add(void* a, void* b)
		{
			return Add((IntPtr) a, (IntPtr) b);
		}

		public static IntPtr Add(IntPtr p, int bytes)
		{
			return p + bytes;
		}

		public static IntPtr Add(IntPtr p, long l)
		{
			long v = p.ToInt64() + l;
			return (IntPtr) v;
		}

		public static IntPtr Add(IntPtr p, IntPtr b)
		{
			return (IntPtr) ((long) p + b.ToInt64());
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
	}

}