using System;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using RazorSharp.Pointers;
using RazorSharp.Utilities;

namespace RazorSharp.Memory
{

	using CSUnsafe = System.Runtime.CompilerServices.Unsafe;

	public static unsafe class Memory
	{
		public static byte[] ReadBytes(IntPtr p, int byteOffset, int size)
		{
			byte[] b = new byte[size];
			for (int i = 0; i < size; i++)
				b[i] = Marshal.ReadByte(p, byteOffset + i);
			return b;
		}

		[HandleProcessCorruptedStateExceptions]
		public static T SafeRead<T>(Pointer<T> ptr, int elemOfs = 0)
		{
			T      t    = default;
			IntPtr addr = Unsafe.Offset<T>(ptr.Address, elemOfs);


			if (Assertion.Throws<NullReferenceException>(delegate { t = CSUnsafe.Read<T>(addr.ToPointer()); })) {
				return default;
			}

			if (Assertion.Throws<AccessViolationException>(delegate { t = CSUnsafe.Read<T>(addr.ToPointer()); })) {
				return default;
			}

			return t;
		}

		[HandleProcessCorruptedStateExceptions]
		public static string SafeToString<T>(IntPtr ptr, int elemOfs = 0)
		{
			string s    = "";
			IntPtr addr = Unsafe.Offset<T>(ptr, elemOfs);
			if (Assertion.Throws<NullReferenceException>(
				delegate { s = CSUnsafe.Read<T>(addr.ToPointer()).ToString(); })) {
				return "(null)";
			}

			if (Assertion.Throws<AccessViolationException>(delegate
			{
				s = CSUnsafe.Read<T>(addr.ToPointer()).ToString();
			})) {
				return "(ave)";
			}

			return s;
		}

		[HandleProcessCorruptedStateExceptions]
		public static string SafeToString<T>(Pointer<T> ptr, int elemOfs = 0)
		{
			return SafeToString<T>(ptr.Address, elemOfs);
		}

		public static void Write<T>(IntPtr p, int byteOffset, T t)
		{
			CSUnsafe.Write((p + byteOffset).ToPointer(), t);
		}

		public static T Read<T>(IntPtr p, int byteOffset)
		{
			return CSUnsafe.Read<T>((p + byteOffset).ToPointer());
		}


		public static void WriteAs<TPtr, TValue>(Pointer<TPtr> ptr, int elemOffsetTValue, TValue v)
		{
			var nPtr = ptr.Reinterpret<TValue>();
			nPtr[elemOffsetTValue] = v;
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