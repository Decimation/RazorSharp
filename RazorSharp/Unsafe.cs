#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using RazorSharp.CLR;
using RazorSharp.CLR.Structures;
using RazorSharp.Pointers;
using RazorSharp.Utilities;

#endregion

namespace RazorSharp
{

	#region

	using CSUnsafe = System.Runtime.CompilerServices.Unsafe;
	using MMemory = Memory.Memory;

	#endregion


	public enum OffsetType
	{
		/// <summary>
		///     Return the pointer offset by <c>-</c><see cref="IntPtr.Size" />,
		///     so it points to the object's <see cref="ObjHeader" />.
		/// </summary>
		Header,

		/// <summary>
		///     If the type is a <c>string</c>, return the
		///     pointer offset by <see cref="RuntimeHelpers.OffsetToStringData" /> so it
		///     points to the string's characters.
		///     <remarks>
		///         Note: Equal to <see cref="GCHandle.AddrOfPinnedObject" /> and <c>fixed</c>.
		///     </remarks>
		/// </summary>
		StringData,

		/// <summary>
		///     If the type is an array, return
		///     the pointer offset by <see cref="Runtime.OffsetToArrayData" /> so it points
		///     to the array's elements.
		///     <remarks>
		///         Note: Equal to <see cref="GCHandle.AddrOfPinnedObject" /> and <c>fixed</c>
		///     </remarks>
		/// </summary>
		ArrayData,

		/// <summary>
		///     If the type is a reference type, return
		///     the pointer offset by <see cref="IntPtr.Size" /> so it points
		///     to the object's fields.
		/// </summary>
		Fields,

		/// <summary>
		///     Don't offset the heap pointer at all, so it
		///     points to the <see cref="MethodTable" /> pointer.
		/// </summary>
		None
	}


	/// <summary>
	///     Provides utilities for manipulating pointers, memory and types
	/// </summary>
	public static unsafe class Unsafe
	{
		internal const int InvalidValue = -1;


		#region OffsetOf

		/// <summary>
		///     Returns the field offset of the specified field, by name.
		/// </summary>
		/// <remarks>
		///     Returned from <see cref="FieldDesc.Offset" />
		/// </remarks>
		/// <param name="fieldName">Name of the field</param>
		/// <typeparam name="TType">Enclosing type</typeparam>
		/// <returns>Field offset</returns>
		public static int OffsetOf<TType>(string fieldName)
		{
			return Runtime.GetFieldDesc<TType>(fieldName)->Offset;
		}

		/// <summary>
		///     Returns the field offset of the specified field, by value.
		/// </summary>
		/// <param name="type">Instance of enclosing type</param>
		/// <param name="val">Value to calculate the offset of</param>
		/// <typeparam name="TType">Enclosing type</typeparam>
		/// <typeparam name="TMember">Member type</typeparam>
		/// <returns>Field offset</returns>
		public static int OffsetOf<TType, TMember>(ref TType type, TMember val)
		{
			int memberSize = SizeOf<TMember>();

			// Find possible matching FieldDesc types
			//var fieldDescs = RRuntime.GetFieldDescs<TType>().Select(x => x.Value)
			//	.Where(x => x.CorType == Constants.TypeToCorType<TMember>()).ToArray();

			// Not using LINQ is faster
			Pointer<FieldDesc>[] fieldDescsPtrs = Runtime.GetFieldDescs<TType>();
			List<FieldDesc>      fieldDescs     = new List<FieldDesc>();
			foreach (Pointer<FieldDesc> p in fieldDescsPtrs) {
				if (p.Reference.CorType == Constants.TypeToCorType<TMember>()) {
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

		public static IntPtr AddressOfFunction<T>(string fnName)
		{
			return Runtime.GetMethodDesc<T>(fnName)->Function;
		}

		/// <summary>
		///     Returns the address of a field in the specified type.
		/// </summary>
		/// <param name="instance">Instance of the enclosing type</param>
		/// <param name="name">Name of the field</param>
		/// <param name="isAutoProperty">Whether the field is an auto-property</param>
		public static IntPtr AddressOfField<T>(ref T instance, string name, bool isAutoProperty = false)
		{
			FieldDesc* fd = Runtime.GetFieldDesc<T>(name, isAutoProperty);


			return fd->GetAddress(ref instance);
		}

		/// <summary>
		///     <para>Returns the address of a type in memory.</para>
		///     <remarks>
		///         <para> Equals <see cref="CSUnsafe.AsPointer{T}" /></para>
		///     </remarks>
		/// </summary>
		/// <param name="t">Type to return the address of</param>
		/// <returns>The address of the type in memory.</returns>
		public static IntPtr AddressOf<T>(ref T t)
		{
			TypedReference tr = __makeref(t);
			return *(IntPtr*) (&tr);
		}

		/// <summary>
		///     Returns the address of a reference type's heap memory.
		///     <remarks>
		///         <para>
		///             Note: This does not pin the reference in memory if it is a reference type.
		///             This may require pinning to prevent the GC from moving the object.
		///             If the GC compacts the heap, this pointer may become invalid if the corresponding reference moves.
		///         </para>
		///     </remarks>
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

		/// <summary>
		///     Returns the address of a reference type's heap memory, offset by the specified <see cref="OffsetType" />.
		///     <remarks>
		///         <para>
		///             Note: This does not pin the reference in memory if it is a reference type.
		///             This may require pinning to prevent the GC from moving the object.
		///             If the GC compacts the heap, this pointer may become invalid if the corresponding reference moves.
		///         </para>
		///     </remarks>
		/// </summary>
		/// <param name="t">Reference type to return the heap address of</param>
		/// <param name="offset">Offset type</param>
		/// <returns>The address of <paramref name="t" /></returns>
		/// <exception cref="ArgumentOutOfRangeException">If <paramref name="offset"></paramref> is out of range.</exception>
		public static IntPtr AddressOfHeap<T>(ref T t, OffsetType offset) where T : class
		{
			switch (offset) {
				case OffsetType.StringData:

					RazorContract.RequiresType<string, T>();
					string s = t as string;
					return AddressOfHeap(ref s) + RuntimeHelpers.OffsetToStringData;

				case OffsetType.ArrayData:
					RazorContract.RequiresType<Array, T>();


					return AddressOfHeap(ref t) + Runtime.OffsetToArrayData;

				case OffsetType.Fields:

					// todo: if the type is an array, should this return ArrayData,
					// todo: ...and if it's a string, should this return StringData?

					// Skip over the MethodTable*
					return AddressOfHeap(ref t) + IntPtr.Size;

				case OffsetType.None:
					return AddressOfHeap(ref t);

				case OffsetType.Header:
					return AddressOfHeap(ref t) - IntPtr.Size;
				default:
					throw new ArgumentOutOfRangeException(nameof(offset), offset, null);
			}
		}

		#endregion

		#region Sizes

		public static int AutoSizeOf<T>(ref T t)
		{
			if (typeof(T).IsValueType) {
				return SizeOf<T>();
			}

			return HeapSizeInternal(ref t);
		}

		/// <summary>
		///     Returns the managed size of an object.
		/// </summary>
		/// <remarks>
		///     Returned from <see cref="EEClassLayoutInfo.ManagedSize" />
		/// </remarks>
		/// <returns><c>-1</c> if <typeparamref name="T" /> is an array; managed size otherwise</returns>
		public static int ManagedSizeOf<T>()
		{
			// No layout
			if (typeof(T).IsArray) {
				return InvalidValue;
			}

			MethodTable* mt = Runtime.MethodTableOf<T>();
			EEClass*     ee = mt->EEClass;
			if (ee->HasLayout) {
				return (int) ee->LayoutInfo->ManagedSize;
			}

			return InvalidValue;
		}

		/// <summary>
		///     <para>Returns the native (<see cref="Marshal" />) size of a type.</para>
		/// </summary>
		/// <remarks>
		///     <para> Returned from <see cref="EEClass.NativeSize" /> </para>
		///     <para> Equals <see cref="Marshal.SizeOf(Type)" /></para>
		///     <para> Equals <see cref="StructLayoutAttribute.Size" /> when type isn't zero-sized.</para>
		/// </remarks>
		/// <returns>The native size if the type has a native representation; -<c>-1</c> otherwise</returns>
		public static int NativeSizeOf<T>()
		{
			// 0
			if (typeof(T).IsArray) {
				return InvalidValue;
			}

			MethodTable* mt        = Runtime.MethodTableOf<T>();
			int          native    = mt->EEClass->NativeSize;
			int          nativeOut = native == 0 ? InvalidValue : native;
			return nativeOut;
		}


		/// <summary>
		///     <para>Returns the size of a type in memory.</para>
		///     <para>Call to <see cref="CSUnsafe.SizeOf{T}()" /></para>
		/// </summary>
		/// <returns><see cref="IntPtr.Size" /> for reference types, size for value types</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int SizeOf<T>()
		{
			return CSUnsafe.SizeOf<T>();
		}


		/// <summary>
		///     <para>Calculates the size of a reference type in heap memory.</para>
		///     <para>This is the most accurate size calculation.</para>
		///     <para>
		///         This follows the size formula of: (<see cref="MethodTable.BaseSize" />) + (length) *
		///         (<see cref="MethodTable.ComponentSize" />)
		///     </para>
		///     <para>where:</para>
		///     <list type="bullet">
		///         <item>
		///             <description>
		///                 <see cref="MethodTable.BaseSize" /> = The base instance size of a type (<c>24</c> (x64) or <c>12</c>
		///                 (x86)
		///                 by default) (<see cref="Constants.MinObjectSize" />)
		///             </description>
		///         </item>
		///         <item>
		///             <description>length	= array or string length; <c>1</c> otherwise</description>
		///         </item>
		///         <item>
		///             <description><see cref="MethodTable.ComponentSize" /> = element size, if available; <c>0</c> otherwise</description>
		///         </item>
		///     </list>
		/// </summary>
		/// <remarks>
		///     <para>Source: /src/vm/object.inl: 45</para>
		///     <para>Equals the Son Of Strike "!do" command.</para>
		///     <para>Equals <see cref="Unsafe.BaseInstanceSize{T}()" /> for objects that aren't arrays or strings.</para>
		///     <para>Note: This also includes padding and overhead.</para>
		/// </remarks>
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

			return HeapSizeInternal(ref t);
		}

		private static int HeapSizeInternal<T>(ref T t)
		{
			// No need to assert, we already know it's not a value type
//			RazorContract.Assert(!typeof(T).IsValueType);

			// We have to manually read the MethodTable because if it's an array,
			// the TypeHandle won't work.
			MethodTable* methodTable = Runtime.ReadMethodTable(ref t);

			if (typeof(T).IsArray) {
				Array arr = t as Array;
				RazorContract.RequiresNotNull(arr);
				return (int) methodTable->BaseSize + arr.Length * methodTable->ComponentSize;
			}

			if (t is string str) {
				return (int) methodTable->BaseSize + str.Length * methodTable->ComponentSize;
			}

			return (int) methodTable->BaseSize;
		}


		/// <summary>
		///     <para>Returns the base size of the fields in the heap.</para>
		///     <para>This follows the formula of:</para>
		///     <para><see cref="MethodTable.BaseSize" /> - <see cref="EEClass.BaseSizePadding" /></para>
		///     <remarks>
		///         <para>Use <see cref="BaseFieldsSize{T}(ref T)" /> if the value may be boxed.</para>
		///         <para>Returned from <see cref="MethodTable.NumInstanceFieldBytes" /></para>
		///     </remarks>
		/// </summary>
		/// <returns><see cref="Constants.MinObjectSize" />if type is an array, fields size otherwise</returns>
		public static int BaseFieldsSize<T>()
		{
			//inline DWORD MethodTable::GetNumInstanceFieldBytes()
			//{
			//	return(GetBaseSize() - GetClass()->GetBaseSizePadding());
			//}

			// When an array MethodTable* is read, its NumInstanceFieldBytes
			// is actually equal to Constants.MinObjectSize although arrays don't have "fields"
			if (typeof(T).IsArray) {
				return Constants.MinObjectSize;
			}

			return Runtime.MethodTableOf<T>()->NumInstanceFieldBytes;
		}

		/// <summary>
		///     <para>Returns the base size of the fields in the heap.</para>
		///     <para>This follows the formula of:</para>
		///     <para><see cref="MethodTable.BaseSize" /> - <see cref="EEClass.BaseSizePadding" /></para>
		///     <para>
		///         Compared to <see cref="BaseFieldsSize{T}()" />, this manually reads the <c>MethodTable*</c>, making
		///         this work for boxed values.
		///     </para>
		///     <code>
		/// object o = 123;
		/// Unsafe.BaseFieldsSize(ref o);  // == 4 (boxed int, base fields size of int (sizeof(int)))
		/// Unsafe.BaseFieldsSize&lt;object&gt;; // == 0 (base fields size of object)
		/// </code>
		///     <remarks>
		///         <para>Returned from <see cref="MethodTable.NumInstanceFieldBytes" /></para>
		///         <para>Equals <see cref="Unsafe.SizeOf{T}()" /> for value types</para>
		///     </remarks>
		/// </summary>
		public static int BaseFieldsSize<T>(ref T t) where T : class
		{
			return Runtime.ReadMethodTable(ref t)->NumInstanceFieldBytes;
		}


		/// <summary>
		///     <para>Returns the base instance size according to the TypeHandle (<c>MethodTable</c>).</para>
		///     <para>This is the minimum heap size of a type.</para>
		///     <para>By default, this equals <see cref="Constants.MinObjectSize" /> (<c>24</c> (x64) or <c>12</c> (x84)).</para>
		/// </summary>
		/// <remarks>
		///     <para>Returned from <see cref="MethodTable.BaseSize" /></para>
		/// </remarks>
		/// <returns><see cref="Constants.MinObjectSize" /> if type is array, base instance size otherwise</returns>
		public static int BaseInstanceSize<T>() where T : class
		{
			// Arrays don't have a TypeHandle, so we have to read the
			// MethodTable* manually. We obviously can't do that here because
			// this method is parameterless.
			if (typeof(T).IsArray) {
				return Constants.MinObjectSize;
			}

			return (int) Runtime.MethodTableOf<T>()->BaseSize;
		}

		#endregion

		#region Misc

		public static bool IsBoxed<T>(in T value)
		{
			return
				(typeof(T).IsInterface || typeof(T) == typeof(object)) &&
				value != null &&
				value.GetType().IsValueType;
		}

		/// <summary>
		///     Copy a managed type's memory into a byte array.
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
			int fieldSize = HeapSize(ref t) - IntPtr.Size * 2;
			Console.WriteLine(fieldSize);
			byte[] fields = new byte[fieldSize];

			// Skip over the MethodTable*
			Marshal.Copy(AddressOfHeap(ref t) + IntPtr.Size, fields, 0, fieldSize);
			return fields;
		}

		/// <summary>
		///     Moves a managed type's data in heap memory.
		/// </summary>
		public static void Move<T>(ref T t, IntPtr newHeapAddr) where T : class
		{
			byte[] heapBytes = MemoryOf(ref t);

			Debug.Assert(heapBytes.Length == HeapSize(ref t));
			MMemory.Zero(AddressOfHeap(ref t) - IntPtr.Size, HeapSize(ref t));
			MMemory.WriteBytes(newHeapAddr, heapBytes);
			IntPtr newAddr = newHeapAddr + IntPtr.Size;
			Marshal.WriteIntPtr(AddressOf(ref t), newAddr);
		}

		public static void WriteReference<T>(ref T t, IntPtr newHeapAddr)
		{
			Marshal.WriteIntPtr(AddressOf(ref t), newHeapAddr);
		}

		public static void WriteReference<T>(ref T t, void* newHeapAddr)
		{
			Marshal.WriteIntPtr(AddressOf(ref t), new IntPtr(newHeapAddr));
		}

		#endregion

	}

}