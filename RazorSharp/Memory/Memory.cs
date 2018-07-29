using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;
using RazorSharp.Pointers;
using RazorSharp.Runtime.CLRTypes;
using RazorSharp.Utilities;

namespace RazorSharp.Memory
{

	using CSUnsafe = System.Runtime.CompilerServices.Unsafe;

	public static unsafe class Memory
	{

		#region Swap

		public static void Swap<T>(void* a, void* b)
		{
			T aval = CSUnsafe.Read<T>(a);
			T bval = CSUnsafe.Read<T>(b);
			CSUnsafe.Write(a, bval);
			CSUnsafe.Write(b, aval);
		}

		public static void Swap<T>(ref T a, ref T b)
		{
			T buf = a;
			a = b;
			b = buf;
		}

		#endregion

		#region Safe

		[HandleProcessCorruptedStateExceptions]
		public static T SafeRead<T>(Pointer<T> ptr, int elemOfs = 0)
		{
			T      t    = default;
			IntPtr addr = Offset<T>(ptr.Address, elemOfs);


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
			IntPtr addr = Offset<T>(ptr, elemOfs);

			if (Assertion.Throws<AccessViolationException, NullReferenceException>(delegate
			{
				s = CSUnsafe.Read<T>(addr.ToPointer()).ToString();
			})) {
				return "(sigsegv)";
			}

			return s;
		}



		[HandleProcessCorruptedStateExceptions]
		public static string SafeToString<T>(Pointer<T> ptr, int elemOfs = 0)
		{
			return SafeToString<T>(ptr.Address, elemOfs);
		}

		#endregion

		#region Array operations

		public static byte[] ReadBytes(IntPtr p, int byteOffset, int size)
		{
			byte[] b = new byte[size];
			for (int i = 0; i < size; i++)
				b[i] = Marshal.ReadByte(p, byteOffset + i);
			return b;
		}
		
		public static void WriteBytes(IntPtr dest, byte[] src)
		{
			for (int i = 0; i < src.Length; i++) {
				Marshal.WriteByte(dest, i, src[i]);
			}
		}

		#endregion



		public static bool ReadBit(int b, int bitIndex)
		{
			//var bit = (b & (1 << bitNumber-1)) != 0;
			return (b & (1 << bitIndex)) != 0;
		}

		public static void Write<T>(IntPtr p, int byteOffset, T t)
		{
			CSUnsafe.Write((p + byteOffset).ToPointer(), t);
		}

		public static T Read<T>(IntPtr p, int byteOffset)
		{
			return CSUnsafe.Read<T>((p + byteOffset).ToPointer());
		}

		public static bool IsValid<T>(IntPtr addrOfPtr) where T : class
		{
			if (addrOfPtr == IntPtr.Zero) return false;

			var validMethodTable = Runtime.Runtime.MethodTableOf<T>();

			var mt = Marshal.ReadIntPtr(addrOfPtr);

			var readMethodTable = *(MethodTable**) mt;


			return readMethodTable->Equals(*validMethodTable);
		}

		public static IntPtr Add(void* v, int bytes)
		{
			return (IntPtr) (((long) v) + bytes);
		}

		public static IntPtr Add(void* a, void* b)
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

		public static void WriteAs<TPtr, TValue>(Pointer<TPtr> ptr, int elemOffsetTValue, TValue v)
		{
			var nPtr = ptr.Reinterpret<TValue>();
			nPtr[elemOffsetTValue] = v;
		}
		

		#region Zero

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

		#endregion


		#region Offset

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IntPtr Offset<T>(IntPtr p, int cnt)
		{
			int size = Unsafe.SizeOf<T>();
			size *= cnt;
			return p + size;
		}

		public static IntPtr Offset<T>(void* p, int cnt)
		{
			return Offset<T>((IntPtr) p,cnt);
		}

		#endregion


	}

}