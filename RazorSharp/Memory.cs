using System;
using System.Runtime.InteropServices;

namespace RazorSharp
{
	using CSUnsafe = System.Runtime.CompilerServices.Unsafe;
	public static unsafe class Memory
	{
		public static byte[] ReadBytes(IntPtr p, int offset, int size)
		{
			byte[] b = new byte[size];
			for (int i = 0; i < size; i++)
				b[i] = Marshal.ReadByte(p, offset + i);
			return b;
		}

		public static void Copy<T>(IntPtr p, T[] @in, int offset)
		{
			for (int i = 0; i < @in.Length; i++) {
				@in[i] = CSUnsafe.Read<T>((void*) ((long) p + (Unsafe.SizeOf<T>() * (offset + i))));
			}
		}

		public static void Write(IntPtr dest, byte[] src)
		{
			for (int i = 0; i < src.Length; i++) {
				Marshal.WriteByte(dest, i, src[i]);
			}
		}

		public static void Zero(IntPtr ptr, int length)
		{
			Zero(ptr.ToPointer(), length);
		}

		public static void Zero(void* ptr, int length)
		{
			byte* memptr = (byte*) ptr;
			for (int i = 0; i < length; i++) {
				memptr[i] = 0;
			}
		}
	}

}