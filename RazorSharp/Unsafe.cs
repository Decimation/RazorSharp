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

		public static IntPtr AddressOfHeap<T>(ref T t, OffsetType offset) where T : class
		{
			switch (offset) {
				case OffsetType.StringData:

					AssertType<string, T>();
					var s = t as string;
					return AddressOfHeap(ref s) + RuntimeHelpers.OffsetToStringData;

				case OffsetType.ArrayData:

					if (!typeof(T).IsArray) {
						TypeException.Throw<Array, T>();
					}

					return AddressOfHeap(ref t) + Runtime.Runtime.OffsetToArrayData;

				case OffsetType.Fields:

					// todo: if the type is an array, should this return ArrayData,
					// todo: ...and if it's a string, should this return StringData?

					// Skip over the MethodTable*
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
		/// Calculates the size of a reference type in heap memory.
		/// This is equivalent to the SOS "!do" command.<para></para>
		///
		/// This is the most accurate size calculation.<para></para>
		///
		/// This follows the size formula of:<para></para>
		///
		/// (base instance size) + (length) * (component size)<para></para>
		///
		/// where:
		/// 	base instance size = The base instance size of a type (24 (x64) or 12 (x86) by default)<para></para>
		/// 	length			   = array or string length, 1 otherwise<para></para>
		/// 	component size	   = element size, if available; 0 otherwise<para></para>
		/// </summary>
		///
		/// Note: this also includes padding.
		/// <returns>The size of the type in heap memory, in bytes</returns>
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

			// From object.h line 65:

			/* 	  The size of the object in the heap must be able to be computed
			 *    very, very quickly for GC purposes.   Restrictions on the layout
			 *    of the object guarantee this is possible.
			 *
			 *    Any object that inherits from Object must be able to
			 *    compute its complete size by using the first 4 bytes of
			 *    the object following the Object part and constants
			 *    reachable from the MethodTable...
			 *
			 *    The formula used for this calculation is:
			 *        MT->GetBaseSize() + ((OBJECTTYPEREF->GetSizeField() * MT->GetComponentSize())
			 *
			 *    So for Object, since this is of fixed size, the ComponentSize is 0, which makes the right side
			 *    of the equation above equal to 0 no matter what the value of GetSizeField(), so the size is just the base size.
			 *
			 */

			var methodTable = Runtime.Runtime.ReadMethodTable(ref t);

			if (typeof(T).IsArray) {
				var arr = t as Array;
				// ReSharper disable once PossibleNullReferenceException
				return (int) methodTable->BaseSize + arr.Length * methodTable->ComponentSize;
			}

			if (t is string str) {
				return (int) methodTable->BaseSize + str.Length * methodTable->ComponentSize;
			}

			return (int) methodTable->BaseSize;
		}

		/// <summary>
		/// Calculates the base size of the fields in the heap minus padding of the base size.
		///
		/// Note: If the fields *themselves* are padded, those are still included.
		/// Note: Doesn't work when T has generic parameters
		/// </summary>
		public static int BaseFieldsSize<T>()
		{
			//inline DWORD MethodTable::GetNumInstanceFieldBytes()
			//{
			//	return(GetBaseSize() - GetClass()->GetBaseSizePadding());
			//}

			if (typeof(T).IsConstructedGenericType) {
				return -1;
			}

			var mt = Runtime.Runtime.MethodTableOf<T>();
			return  (int) mt->BaseSize - mt->EEClass->BaseSizePadding;
		}

		/// <summary>
		/// Returns the base instance size according to the TypeHandle (MethodTable).
		/// This is the minimum heap size of a type.
		/// </summary>
		/// <returns>-1 if type is array, base instance size otherwise</returns>
		public static int BaseInstanceSize<T>() where T : class
		{
			// Arrays don't have a TypeHandle, so we have to read the
			// MethodTable* manually. We obviously can't do that here because
			// this method is parameterless.
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
			Memory.Memory.Zero(AddressOfHeap(ref t) - IntPtr.Size, HeapSize(ref t));
			Memory.Memory.WriteBytes(newHeapAddr, heapBytes);
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
				// ReSharper disable once PossibleNullReferenceException
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


		public static void WriteReference<T>(ref T t, IntPtr newHeapAddr)
		{
			Marshal.WriteIntPtr(Unsafe.AddressOf(ref t), newHeapAddr);
		}

		public static void WriteReference<T>(ref T t, void* newHeapAddr)
		{
			Marshal.WriteIntPtr(Unsafe.AddressOf(ref t), new IntPtr(newHeapAddr));
		}
	}

}