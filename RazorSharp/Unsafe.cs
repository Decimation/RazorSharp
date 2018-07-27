using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using RazorCommon;
using RazorSharp.Utilities;
using static RazorSharp.Utilities.Assertion;

namespace RazorSharp
{
	using CSUnsafe = System.Runtime.CompilerServices.Unsafe;

	public enum OffsetType
	{
		/// <summary>
		/// If the type is a string, return the
		/// pointer offset by RuntimeHelpers.OffsetToStringData so it
		/// points to the string's characters.
		///
		/// Note: Equivalent to GCHandle.AddrOfPinnedObject and fixed.
		/// </summary>
		StringData,

		/// <summary>
		/// If the type is an array, return
		/// the pointer offset by IntPtr.Size * 2 so it points
		/// to the array's elements.
		/// </summary>
		ArrayData,

		/// <summary>
		/// If the type is a reference type, return
		/// the pointer offset by IntPtr.Size so it points
		/// to the object's fields.
		/// </summary>
		Fields,

		/// <summary>
		/// Don't offset the heap pointer at all, so it
		/// points to the MethodTable*.
		/// </summary>
		None
	}


	/// <summary>
	/// Provides utilities for manipulating pointers, memory and types
	/// </summary>
	public static unsafe class Unsafe
	{
		#region Address

		/// <summary>
		/// Returns the address of a type in memory.
		///
		/// Note: This does not pin the reference in memory if it is a reference type.
		/// </summary>
		/// <param name="t">Type to return the address of</param>
		/// <returns>The address of the type in memory.</returns>
		public static IntPtr AddressOf<T>(ref T t)
		{
			TypedReference tr = __makeref(t);
			return *(IntPtr*) (&tr);
		}

		/// <summary>
		/// Returns the address of a reference type's heap memory.
		///
		/// Note: This does not pin the reference in memory if it is a reference type.
		/// </summary>
		/// <param name="t">Reference type to return the heap address of</param>
		/// <returns>The address of the heap object.</returns>
		public static IntPtr AddressOfHeap<T>(ref T t) where T : class
		{

			TypedReference tr = __makeref(t);

			// NOTE:
			// Strings have their data offset by RuntimeHelpers.OffsetToStringData
			// Arrays have their data offset by IntPtr.Size * 2 bytes (may be different for 32 bit)
			return **(IntPtr**) (&tr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IntPtr Offset<T>(IntPtr p, int cnt)
		{
			int size = SizeOf<T>();
			size *= cnt;
			return p + size;
		}

		public static IntPtr Offset<T>(void* p, int cnt)
		{
			return Offset<T>((IntPtr) p,cnt);
		}

		public static IntPtr AddressOfHeap<T>(ref T t, OffsetType offset) where T : class
		{
			switch (offset) {
				case OffsetType.StringData:

					AssertType<string, T>();
					var s = t as string;
					fixed (char* c = s) {
						return (IntPtr) c;
					}

				case OffsetType.ArrayData:

					if (!typeof(T).IsArray) {
						TypeException.Throw<Array, T>();
					}

					return AddressOfHeap(ref t) + IntPtr.Size * 2;

				case OffsetType.Fields:

					// todo: if the type is an array, should this return ArrayData,
					// todo: ...and if it's a string, should this return StringData?
					return AddressOfHeap(ref t) + IntPtr.Size;

				case OffsetType.None:
					return AddressOfHeap(ref t);

				default:
					throw new ArgumentOutOfRangeException(nameof(offset), offset, null);
			}
		}

		#endregion

		#region Sizes

		/// <summary>
		/// Calculates the size of a type in stack memory.
		/// (Call to CompilerServices.Unsafe.SizeOf)
		/// </summary>
		/// <returns>IntPtr.Size for reference types, size in stack for value types</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int SizeOf<T>()
		{
			return CSUnsafe.SizeOf<T>();
		}

		/// <summary>
		/// Calculates the size of a reference type's heap memory.
		/// This is equivalent to the SOS "!do" command.
		///
		/// This is the most accurate size calculation.
		///
		/// This follows the size formula of:
		///
		/// (base instance size) + (length) * (component size)
		///
		/// where:
		/// 	base instance size = 24 (x64) or 12 (x86)
		/// 	length			   = array or string length, 1 otherwise
		/// 	component size	   = element size, if available
		/// </summary>
		///
		/// Note: this also includes padding.
		/// <returns>The size of the type's heap memory, in bytes</returns>
		public static int HeapSize<T>(ref T t) where T : class
		{
			/**
			 * Type			x86 size				x64 size
			 *
			 * object		12						24
			 * object[]		16 + length * 4			32 + length * 8
			 * int[]		12 + length * 4			28 + length * 4
			 * byte[]		12 + length				24 + length
			 * string		14 + length * 2			26 + length * 2
			 */

			var methodTable = Runtime.Runtime.ReadMethodTable(ref t);

			if (typeof(T).IsArray) {
				var arr = t as Array;
				return (int) methodTable->BaseSize + arr.Length * methodTable->ComponentSize;
			}

			if (t is string) {
				var str = t as string;
				return (int) methodTable->BaseSize + str.Length * methodTable->ComponentSize;
			}

			return (int) methodTable->BaseSize;
		}

		/// <summary>
		/// Calculates the base size of the fields in the heap minus padding of the base size.
		///
		/// Note that if the fields *themselves* are padded, those are still included.
		/// </summary>
		public static int BaseFieldsSize<T>()
		{
			var mt = Runtime.Runtime.MethodTableOf<T>();
			return  (int) mt->BaseSize - mt->EEClass->BaseSizePadding;
		}

		/// <summary>
		/// Returns the base instance size according to the TypHandle (MethodTable).
		/// </summary>
		/// <returns>-1 if type is array, base instance size otherwise</returns>
		public static int BaseInstanceSize<T>() where T : class
		{
			if (typeof(T).IsArray) return -1;
			return (int) Runtime.Runtime.MethodTableOf<T>()->BaseSize;
		}



		#endregion

		#region Misc

		/// <summary>
		/// Copy a managed type's memory into a byte array.
		/// </summary>
		/// <returns>A byte array containing the managed type's raw memory</returns>
		public static byte[] MemoryOf<T>(ref T t) where T : class
		{
			int    heapSize = HeapSize(ref t);
			byte[] alloc    = new byte[heapSize];

			// Need to include the ObjHeader
			Marshal.Copy(AddressOfHeap(ref t) - IntPtr.Size, alloc, 0, heapSize);
			return alloc;
		}

		/// <summary>
		/// Moves a managed type's data in heap memory.
		/// </summary>
		public static void Move<T>(ref T t, IntPtr newHeapAddr) where T : class
		{
			var heapBytes  = MemoryOf(ref t);
			Debug.Assert(heapBytes.Length == HeapSize(ref t));
			Memory.Zero(AddressOfHeap(ref t) - IntPtr.Size, HeapSize(ref t));
			Memory.Write(newHeapAddr, heapBytes);
			IntPtr newAddr = newHeapAddr + IntPtr.Size;
			Marshal.WriteIntPtr(AddressOf(ref t), newAddr);
		}

		/// <summary>
		/// Determines whether a type is blittable, that is, they don't
		/// require conversion between managed and unmanaged code.
		/// </summary>
		public static bool IsBlittable<T>()
		{
			return IsBlittable(typeof(T));
		}

		private static bool IsBlittable(Type t)
		{
			if (t.IsArray) {
				var elem = t.GetElementType();
				return elem.IsValueType && IsBlittable(elem);
			}

			if (t == typeof(string))
				return true;

			try {
				object instance = FormatterServices.GetUninitializedObject(t);
				GCHandle.Alloc(instance, GCHandleType.Pinned).Free();
				return true;
			}
			catch (MemberAccessException) {
				// Type is abstract
				return false;
			}
			catch (ArgumentException) {
				// Type is not blittable
				return false;
			}
		}

		#endregion


	}

}