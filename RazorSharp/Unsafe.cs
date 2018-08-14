#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using RazorSharp.Pointers;
using RazorSharp.Runtime;
using RazorSharp.Runtime.CLRTypes;
using RazorSharp.Utilities;
using static RazorSharp.Utilities.Assertion;

#endregion

namespace RazorSharp
{

	#region

	using CSUnsafe = System.Runtime.CompilerServices.Unsafe;
	using RRuntime = Runtime.Runtime;
	using MMemory = Memory.Memory;

	#endregion


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
		internal const int InvalidValue = -1;


		#region OffsetOf

		public static int OffsetOf<TType>(string fieldName)
		{
			return RRuntime.GetFieldDesc<TType>(fieldName)->Offset;
		}

		public static int OffsetOf<TType, TMember>(ref TType type, TMember val)
		{
			int memberSize = SizeOf<TMember>();

			// Find possible matching FieldDesc types
			//var fieldDescs = RRuntime.GetFieldDescs<TType>().Select(x => x.Value)
			//	.Where(x => x.CorType == Constants.TypeToCorType<TMember>()).ToArray();

			// Not using LINQ is faster
			var fieldDescsPtrs = RRuntime.GetFieldDescs<TType>();
			var fieldDescs     = new List<FieldDesc>();
			foreach (var p in fieldDescsPtrs) {
				if (p.Reference.CorType == Constants.TypeToCorType<TMember>()) {
					// Special note:
					// we can dereference here because we don't access any address-sensitive fields
					fieldDescs.Add(p.Reference);
				}
			}


			Pointer<TMember> rawMemory = AddressOf(ref type);

			if (!typeof(TType).IsValueType) {
				rawMemory = Marshal.ReadIntPtr(rawMemory.Address) + IntPtr.Size;
			}


			foreach (FieldDesc t in fieldDescs) {
				int adjustedOfs = t.Offset / memberSize;
				if (rawMemory[adjustedOfs].Equals(val)) {
					return t.Offset;
				}
			}


			return InvalidValue;
		}

		#endregion


		#region Address


		/// <summary>
		/// Returns the address of a field in the specified type.
		/// </summary>
		/// <param name="instance">Instance of the enclosing type</param>
		/// <param name="name">Name of the field</param>
		/// <param name="isAutoProperty">Whether the field is an auto-property</param>
		public static IntPtr AddressOfField<T>(ref T instance, string name, bool isAutoProperty = false)
		{
			if (isAutoProperty) {
				name = AutoPropertyBackingFieldName(name);
			}

			var fd = RRuntime.GetFieldDesc<T>(name);

			return fd->GetAddress(ref instance);
		}

		/// <summary>
		/// Returns the address of a type in memory.<para></para>
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

					return AddressOfHeap(ref t) + RRuntime.OffsetToArrayData;

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
		/// Calculates the native (Marshal) size of a type. <para></para>
		/// Corresponds to Marshal.SizeOf.
		/// </summary>
		/// <returns>The native size if the type has a native representation; -1 otherwise</returns>
		public static int NativeSizeOf<T>()
		{
			if (typeof(T).IsArray) return InvalidValue;
			var mt     = RRuntime.MethodTableOf<T>();
			var native = mt->EEClass->NativeSize;
			return native == 0 ? InvalidValue : native;
		}

		/// <summary>
		/// Calculates the size of a type in memory.<para></para>
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
		/// <para>where:</para>
		/// 	base instance size = The base instance size of a type (24 (x64) or 12 (x86) by default)<para></para>
		/// 	length			   = array or string length, 1 otherwise<para></para>
		/// 	component size	   = element size, if available; 0 otherwise<para></para>
		/// </summary>
		///
		/// <para>Note: this also includes padding.</para>
		/// <para>Note: Equals BaseInstanceSize for objects that aren't arrays or strings.</para>
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

			// We have to manually read the MethodTable because if it's an array,
			// the TypeHandle won't work.
			var methodTable = RRuntime.ReadMethodTable(ref t);

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

		public static bool IsBoxed<T>(in T value)
		{
			return
				(typeof(T).IsInterface || typeof(T) == typeof(object)) &&
				value != null &&
				value.GetType().IsValueType;
		}

		/// <summary>
		/// <para>Calculates the base size of the fields in the heap minus padding and overhead of the base size.</para>
		///
		/// <para>Note: If the fields *themselves* are padded, those bytes are still included.</para>
		/// <para>Note: Equals SizeOf&lt;T&gt; for value types</para>
		/// </summary>
		public static int BaseFieldsSize<T>()
		{
			//inline DWORD MethodTable::GetNumInstanceFieldBytes()
			//{
			//	return(GetBaseSize() - GetClass()->GetBaseSizePadding());
			//}

			if (typeof(T).IsArray) {
				return InvalidValue;
			}

			var mt = RRuntime.MethodTableOf<T>();
			return (int) mt->NumInstanceFieldBytes;
		}

		/// <summary>
		/// Returns the base instance size according to the TypeHandle (MethodTable).
		/// This is the minimum heap size of a type.<para></para>
		///
		/// </summary>
		/// <returns>-1 if type is array, base instance size otherwise</returns>
		public static int BaseInstanceSize<T>() where T : class
		{
			// Arrays don't have a TypeHandle, so we have to read the
			// MethodTable* manually. We obviously can't do that here because
			// this method is parameterless.
			if (typeof(T).IsArray) return InvalidValue;
			return (int) RRuntime.MethodTableOf<T>()->BaseSize;
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

		public static byte[] MemoryOfVal<T>(ref T t) where T : struct
		{
			int    size  = SizeOf<T>();
			byte[] alloc = new byte[size];
			Marshal.Copy(AddressOf(ref t), alloc, 0, size);
			return alloc;
		}

		public static byte[] MemoryOfFields<T>(ref T t) where T : class
		{
			// Subtract the size of the ObjHeader and MethodTable*
			int    fieldSize = HeapSize(ref t) - (IntPtr.Size * 2);
			byte[] fields    = new byte[fieldSize];

			// Skip over the MethodTable*
			Marshal.Copy(AddressOfHeap(ref t) + IntPtr.Size, fields, 0, fieldSize);
			return fields;
		}

		/// <summary>
		/// Moves a managed type's data in heap memory.
		/// </summary>
		public static void Move<T>(ref T t, IntPtr newHeapAddr) where T : class
		{
			var heapBytes = MemoryOf(ref t);
			Debug.Assert(heapBytes.Length == HeapSize(ref t));
			MMemory.Zero(AddressOfHeap(ref t) - IntPtr.Size, HeapSize(ref t));
			MMemory.WriteBytes(newHeapAddr, heapBytes);
			IntPtr newAddr = newHeapAddr + IntPtr.Size;
			Marshal.WriteIntPtr(AddressOf(ref t), newAddr);
		}

		#endregion

		/// <summary>
		/// Gets the internal name of an auto-property's backing field.
		/// <example>If the auto-property's name is X, the backing field name is &lt;X&gt;k__BackingField.</example>
		/// </summary>
		/// <param name="propname">Auto-property's name</param>
		/// <returns>Internal name of the auto-property's backing field</returns>
		private static string AutoPropertyBackingFieldName(string propname)
		{
			const string backingFieldFormat = "<{0}>k__BackingField";
			return String.Format(backingFieldFormat, propname);
		}

		public static void WriteReference<T>(ref T t, IntPtr newHeapAddr)
		{
			Marshal.WriteIntPtr(AddressOf(ref t), newHeapAddr);
		}

		public static void WriteReference<T>(ref T t, void* newHeapAddr)
		{
			Marshal.WriteIntPtr(AddressOf(ref t), new IntPtr(newHeapAddr));
		}


	}

}