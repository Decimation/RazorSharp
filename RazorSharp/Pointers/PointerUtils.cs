using System;
using System.Runtime.CompilerServices;

namespace RazorSharp.Pointers
{

	public static class PointerUtils
	{
		public static unsafe IntPtr Add(void* v, int bytes)
		{
			return (IntPtr) (((long) v) + bytes);
		}

		public static unsafe IntPtr Add(void* a, void* b)
		{
			return Add((IntPtr) a, (IntPtr) b);
		}

		public static IntPtr Add(IntPtr p, int bytes)
		{
			return (IntPtr) (((long) p) + bytes);
		}

		public static IntPtr Add(IntPtr p, IntPtr b)
		{
			return (IntPtr) (((long) p) + b.ToInt64());
		}

		/// <summary>
		/// Offsets a pointer by cnt elements.
		/// </summary>
		/// <param name="p">Pointer</param>
		/// <param name="elemCnt">Elements to offset by</param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IntPtr Offset<T>(IntPtr p, int elemCnt)
		{
			int size = Unsafe.SizeOf<T>();
			size *= elemCnt;
			return p + size;
		}

		public static int OffsetOf(IntPtr lo, IntPtr hi)
		{
			return (int) (hi.ToInt64() - lo.ToInt64());
		}

		public static unsafe IntPtr Offset<T>(void* p, int elemCnt)
		{
			return Offset<T>((IntPtr) p,elemCnt);
		}
	}

}