#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using RazorSharp.CLR;
using RazorSharp.CLR.Structures;
using RazorSharp.CLR.Structures.EE;
using RazorSharp.Pointers;
using RazorSharp.Utilities;

#endregion

namespace RazorSharp
{

	#region

	using CSUnsafe = System.Runtime.CompilerServices.Unsafe;

	#endregion


	/// <summary>
	///     Offset options for <see cref="Unsafe.AddressOfHeap{T}(ref T, OffsetType)" />
	/// </summary>
	public enum OffsetType
	{
		/// <summary>
		///     Return the pointer offset by <c>-</c><see cref="IntPtr.Size" />,
		///     so it points to the object's <see cref="ObjHeader" />.
		/// </summary>
		Header,

		/// <summary>
		///     If the type is a <see cref="string" />, return the
		///     pointer offset by <see cref="RuntimeHelpers.OffsetToStringData" /> so it
		///     points to the string's characters.
		///     <remarks>
		///         Note: Equal to <see cref="GCHandle.AddrOfPinnedObject" /> and <c>fixed</c>.
		///     </remarks>
		/// </summary>
		StringData,

		/// <summary>
		///     If the type is an array, return
		///     the pointer offset by <see cref="Offsets.OffsetToArrayData" /> so it points
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
	///     Provides utilities for manipulating pointers, memory, and types
	/// </summary>
	public static unsafe class Unsafe
	{
		internal const int INVALID_VALUE = -1;


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
			return Runtime.GetFieldDesc<TType>(fieldName).Reference.Offset;
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


			Pointer<TMember> rawMemory = AddressOf(ref type).Address;

			if (!typeof(TType).IsValueType) {
				rawMemory = Marshal.ReadIntPtr(rawMemory.Address) + IntPtr.Size;
			}

			foreach (FieldDesc t in fieldDescs) {
				int adjustedOfs = t.Offset / memberSize;
				if (rawMemory[adjustedOfs].Equals(val)) {
					return t.Offset;
				}
			}


			return INVALID_VALUE;
		}

		#endregion

		#region Address

		/// <summary>
		///     Returns the address of a field in the specified type.
		/// </summary>
		/// <param name="instance">Instance of the enclosing type</param>
		/// <param name="name">Name of the field</param>
		/// <param name="fieldTypes">If the field has unique attributes (i.e. auto-property)</param>
		public static Pointer<byte> AddressOfField<T>(ref T instance, string name,
			SpecialFieldTypes fieldTypes = SpecialFieldTypes.None)
		{
			Pointer<FieldDesc> fd = Runtime.GetFieldDesc<T>(name, fieldTypes);
			return fd.Reference.GetAddress(ref instance);
		}

		/// <summary>
		///     <para>Returns the address of <paramref name="t" />.</para>
		///     <remarks>
		///         <para> Equals <see cref="CSUnsafe.AsPointer{T}" /></para>
		///     </remarks>
		/// </summary>
		/// <param name="t">Type to return the address of</param>
		/// <returns>The address of the type in memory.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Pointer<T> AddressOf<T>(ref T t)
		{
			TypedReference tr = __makeref(t);
			return *(IntPtr*) (&tr);
		}

		/// <summary>
		///     Returns the address of reference type <paramref name="t" />'s heap memory (raw data).
		///     <remarks>
		///         <para>
		///             Note: This does not pin the reference in memory if it is a reference type.
		///             This may require pinning to prevent the GC from moving the object.
		///             If the GC compacts the heap, this pointer may become invalid.
		///         </para>
		///     </remarks>
		/// </summary>
		/// <param name="t">Reference type to return the heap address of</param>
		/// <returns>The address of the heap object.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Pointer<byte> AddressOfHeap<T>(ref T t) where T : class
		{
			TypedReference tr = __makeref(t);

			// NOTE:
			// Strings have their data offset by RuntimeHelpers.OffsetToStringData
			// Arrays have their data offset by IntPtr.Size * 2 bytes (may be different for 32 bit)
			return **(IntPtr**) (&tr);
		}

		/// <summary>
		///     Returns the address of reference type <paramref name="t" />'s heap memory, offset by the specified
		///     <see cref="OffsetType" />.
		///     <remarks>
		///         <para>
		///             Note: This does not pin the reference in memory if it is a reference type.
		///             This may require pinning to prevent the GC from moving the object.
		///             If the GC compacts the heap, this pointer may become invalid.
		///         </para>
		///     </remarks>
		/// </summary>
		/// <param name="t">Reference type to return the heap address of</param>
		/// <param name="offset">Offset type</param>
		/// <returns>The address of <paramref name="t" /></returns>
		/// <exception cref="ArgumentOutOfRangeException">If <paramref name="offset"></paramref> is out of range.</exception>
		public static Pointer<byte> AddressOfHeap<T>(ref T t, OffsetType offset) where T : class
		{
			switch (offset) {
				case OffsetType.StringData:

					RazorContract.RequiresType<string, T>();
					string s = t as string;
					return AddressOfHeap(ref s) + RuntimeHelpers.OffsetToStringData;

				case OffsetType.ArrayData:

					RazorContract.RequiresType<Array, T>();
					return AddressOfHeap(ref t) + Offsets.OffsetToArrayData;

				case OffsetType.Fields:

					// todo: if the type is an array, should this return ArrayData,
					// todo: ...and if it's a string, should this return StringData?

					// Skip over the MethodTable*
					return AddressOfHeap(ref t) + sizeof(MethodTable*);

				case OffsetType.None:
					return AddressOfHeap(ref t);

				case OffsetType.Header:
					return AddressOfHeap(ref t) - sizeof(ObjHeader);
				default:
					throw new ArgumentOutOfRangeException(nameof(offset), offset, null);
			}
		}

		public static Pointer<byte> AddressOfHeap<T>(T t) where T : class
		{
			return AddressOfHeap(ref t);
		}

		public static Pointer<byte> AddressOfHeap<T>(T t, OffsetType offset) where T : class
		{
			return AddressOfHeap(ref t, offset);
		}

		public static Pointer<byte> AddressOfFunction<T>(string name)
		{
			return AddressOfFunction(typeof(T), name);
		}

		/// <summary>
		/// Returns the entry point of the specified function (assembly code).
		/// </summary>
		/// <param name="t">Enclosing type</param>
		/// <param name="name">Name of the function in <see cref="Type"/> <paramref name="t"/></param>
		/// <returns></returns>
		public static Pointer<byte> AddressOfFunction(Type t, string name)
		{
			var md = Runtime.GetMethodDesc(t, name);

			// Function must be jitted
			Debug.Assert(md.Reference.IsPointingToNativeCode);

			return Runtime.GetMethodDesc(t, name).Reference.Function;
		}

		/*public static Pointer<T> AddressOfHeap<T>(T[] rg)
		{
			return AddressOfHeap(rg, OffsetType.ArrayData).Reinterpret<T>();
		}*/

		#endregion

		#region Sizes

		// todo: make an AutoSize method

		/*public enum SizeType
		{
			/// <summary>
			/// Requires an argument
			/// <see cref="Unsafe.AutoSizeOf{T}"/>
			/// </summary>
			Auto,

			/// <summary>
			/// <see cref="Unsafe.SizeOf{T}"/>
			/// </summary>
			Default,

			/// <summary>
			/// <see cref="Unsafe.ManagedSizeOf{T}"/>
			/// </summary>
			Managed,

			/// <summary>
			/// <see cref="Unsafe.NativeSizeOf{T}"/>
			/// </summary>
			Native,

			/// <summary>
			/// Requires an argument
			/// <see cref="Unsafe.HeapSize{T}"/>
			/// </summary>
			Heap,

			/// <summary>
			/// <see cref="Unsafe.BaseInstanceSize{T}"/>
			/// </summary>
			BaseInstance,

			/// <summary>
			/// Requires an argument
			/// <see cref="Unsafe.BaseFieldsSize{T}(T)"/>
			/// </summary>
			BaseFields,

			/// <summary>
			/// <see cref="Unsafe.BaseFieldsSize{T}()"/>
			/// </summary>
			BaseFields2,
		}

		// todo: WIP

		public static int SizeOf__<T>(T t = default, SizeType type = SizeType.Auto)
		{
			switch (type) {
				case SizeType.Auto:
					return AutoSizeOf(t);
				case SizeType.Default:
					return SizeOf<T>();
				case SizeType.Managed:
					return ManagedSizeOf<T>();
				case SizeType.Native:
					return NativeSizeOf<T>();
				case SizeType.Heap:
					return HeapSizeInternal(t);
				case SizeType.BaseInstance:
					return BaseInstanceSizeInternal<T>();
				case SizeType.BaseFields:
					return BaseFieldsSizeInternal(t);
				case SizeType.BaseFields2:
					return BaseFieldsSize<T>();

				default:
					throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}
		}*/


		/*static int SizeOf<T>(this T val)
		{
			return AutoSizeOf(val);
		}*/

		/// <summary>
		///     Calculates the complete size of <paramref name="t" />'s data. If <typeparamref name="T" /> is
		///     a value type, this is equal to <see cref="SizeOf{T}" />. If <typeparamref name="T" /> is a
		///     reference type, this is equal to <see cref="HeapSize{T}(ref T)" />.
		/// </summary>
		/// <param name="t">Value to calculate the size of</param>
		/// <typeparam name="T">Type of <paramref name="t" /></typeparam>
		/// <returns>The complete size of <paramref name="t" /></returns>
		public static int AutoSizeOf<T>(T t)
		{
			return typeof(T).IsValueType ? SizeOf<T>() : HeapSizeInternal(t);
		}

		/// <summary>
		///     Returns the managed size of an object.
		/// </summary>
		/// <remarks>
		///     Returned from <see cref="EEClassLayoutInfo.ManagedSize" />
		/// </remarks>
		/// <returns>Managed size if the type has an <see cref="EEClassLayoutInfo" />; <see cref="INVALID_VALUE" /> otherwise</returns>
		public static int ManagedSizeOf<T>()
		{
			// Note: Arrays have no layout


			Pointer<MethodTable> mt = Runtime.MethodTableOf<T>();
			Pointer<EEClass>     ee = mt.Reference.EEClass;
			if (ee.Reference.HasLayout) {
				return (int) ee.Reference.LayoutInfo->ManagedSize;
			}

			return INVALID_VALUE;
		}

		/// <summary>
		///     <para>Returns the native (<see cref="Marshal" />) size of a type.</para>
		/// </summary>
		/// <remarks>
		///     <para> Returned from <see cref="EEClass.NativeSize" /> </para>
		///     <para> Equals <see cref="Marshal.SizeOf(Type)" /></para>
		///     <para> Equals <see cref="StructLayoutAttribute.Size" /> when type isn't zero-sized.</para>
		/// </remarks>
		/// <returns>The native size if the type has a native representation; <see cref="INVALID_VALUE" /> otherwise</returns>
		public static int NativeSizeOf<T>()
		{
			// Note: Arrays native size == 0


			Pointer<MethodTable> mt     = Runtime.MethodTableOf<T>();
			int                  native = mt.Reference.EEClass.Reference.NativeSize;

			return native == 0 ? INVALID_VALUE : native;
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

		#region HeapSize

		/// <summary>
		///     <para>Calculates the complete size of a reference type in heap memory.</para>
		///     <para>This is the most accurate size calculation.</para>
		///     <para>
		///         This follows the size formula of: (<see cref="MethodTable.BaseSize" />) + (length) *
		///         (<see cref="MethodTable.ComponentSize" />)
		///     </para>
		///     <para>where:</para>
		///     <list type="bullet">
		///         <item>
		///             <description>
		///                 <see cref="MethodTable.BaseSize" /> = The base instance size of a type
		///                 (<c>24</c> (x64) or <c>12</c> (x86) by default) (<see cref="Constants.MinObjectSize" />)
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
		///     <para>Note: This also includes padding and overhead (<see cref="ObjHeader" /> and <see cref="MethodTable" /> ptr.)</para>
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

			return HeapSizeInternal(t);
		}

		public static int HeapSize<T>(T t) where T : class
		{
			return HeapSize(ref t);
		}

		private static int HeapSizeInternal<T>(T t)
		{
			RazorContract.RequiresClassType<T>();

			// By manually reading the MethodTable*, we can calculate the size correctly if the reference
			// is boxed or cloaked
			Pointer<MethodTable> methodTable = Runtime.ReadMethodTable(ref t);

			if (typeof(T).IsArray) {
				Array arr = t as Array;

				// ReSharper disable once PossibleNullReferenceException
				// We already know it's not null because the type is an array.
				return CalculateHeapSize(arr.Length);
			}

			if (t is string str) {
				return CalculateHeapSize(str.Length);
			}


			int CalculateHeapSize(int length)
			{
				return methodTable.Reference.BaseSize + length * methodTable.Reference.ComponentSize;
			}

			return methodTable.Reference.BaseSize;
		}

		#endregion

		#region BaseFieldsSize

		/// <summary>
		///     <para>Returns the base size of the fields (data) in the heap.</para>
		///     <para>This follows the formula of:</para>
		///     <para><see cref="MethodTable.BaseSize" /> - <see cref="EEClass.BaseSizePadding" /></para>
		///     <remarks>
		///         <para>Use <see cref="BaseFieldsSize{T}(T)" /> if the value may be boxed.</para>
		///         <para>Returned from <see cref="MethodTable.NumInstanceFieldBytes" /></para>
		///         <para>This includes field padding.</para>
		///     </remarks>
		/// </summary>
		/// <returns><see cref="Constants.MinObjectSize" /> if type is an array, fields size otherwise</returns>
		public static int BaseFieldsSize<T>()
		{
			//inline DWORD MethodTable::GetNumInstanceFieldBytes()
			//{
			//	return(GetBaseSize() - GetClass()->GetBaseSizePadding());
			//}


			return Runtime.MethodTableOf<T>().Reference.NumInstanceFieldBytes;
		}

		/// <summary>
		///     <para>Returns the base size of the fields (data) in the heap.</para>
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
		///         <para>This includes field padding.</para>
		///     </remarks>
		/// </summary>
		public static int BaseFieldsSize<T>(T t) where T : class
		{
			return BaseFieldsSizeInternal(t);
		}

		private static int BaseFieldsSizeInternal<T>(T t)
		{
			RazorContract.RequiresClassType<T>();
			return Runtime.ReadMethodTable(ref t).Reference.NumInstanceFieldBytes;
		}

		#endregion

		#region BaseInstanceSize

		/// <summary>
		///     <para>Returns the base instance size according to the TypeHandle (<c>MethodTable</c>).</para>
		///     <para>This is the minimum heap size of a type.</para>
		///     <para>By default, this equals <see cref="Constants.MinObjectSize" /> (<c>24</c> (x64) or <c>12</c> (x84)).</para>
		/// </summary>
		/// <remarks>
		///     <para>Returned from <see cref="MethodTable.BaseSize" /></para>
		/// </remarks>
		/// <returns>
		///     <see cref="MethodTable.BaseSize" />
		/// </returns>
		public static int BaseInstanceSize<T>() where T : class
		{
			return BaseInstanceSizeInternal<T>();
		}

		private static int BaseInstanceSizeInternal<T>()
		{
			RazorContract.RequiresClassType<T>();
			return Runtime.MethodTableOf<T>().Reference.BaseSize;
		}

		#endregion

		#endregion

		#region Misc

		public static bool IsBoxed<T>(in T value)
		{
			return
				(typeof(T).IsInterface || typeof(T) == typeof(object)) &&
				value != null && value.GetType().IsValueType;
		}

		/// <summary>
		///     Copies the memory of <paramref name="t" /> into a <see cref="Byte" /> array.
		///     <remarks>
		///         This includes the <see cref="MethodTable" /> pointer and <see cref="ObjHeader" />
		///     </remarks>
		/// </summary>
		/// <param name="t">Value to copy the memory of</param>
		/// <typeparam name="T">Reference type</typeparam>
		/// <returns>An array of <see cref="Byte" />s containing the raw memory of <paramref name="t" /></returns>
		public static byte[] MemoryOf<T>(T t) where T : class
		{
			// Need to include the ObjHeader
			Pointer<T> ptr = AddressOfHeap(ref t, OffsetType.Header).Address;
			return ptr.CopyOut<byte>(HeapSize(ref t));
		}

		/// <summary>
		///     Copies the memory of <paramref name="t" /> into a <see cref="Byte" /> array.
		///     If <typeparamref name="T" /> is a pointer, the memory of the pointer will be returned.
		/// </summary>
		/// <param name="t">Value to copy the memory of</param>
		/// <typeparam name="T">Value type</typeparam>
		/// <returns>An array of <see cref="Byte" />s containing the raw memory of <paramref name="t" /></returns>
		public static byte[] MemoryOfVal<T>(T t)
		{
			Pointer<T> ptr = AddressOf(ref t);
			return ptr.CopyOut<byte>(ptr.ElementSize);
		}

		public static byte[] MemoryOfFields<T>(T t) where T : class
		{
			// Subtract the size of the ObjHeader and MethodTable*
			int fieldSize = HeapSize(ref t) - IntPtr.Size * 2;

			byte[] fields = new byte[fieldSize];

			// Skip over the MethodTable*
			Marshal.Copy((AddressOfHeap(ref t) + IntPtr.Size).Address, fields, 0, fieldSize);
			return fields;
		}

		internal static void WriteReference<T>(ref T t, IntPtr newHeapAddr)
		{
			Marshal.WriteIntPtr(AddressOf(ref t).Address, newHeapAddr);
		}

		internal static void WriteReference<T>(ref T t, void* newHeapAddr)
		{
			Marshal.WriteIntPtr(AddressOf(ref t).Address, new IntPtr(newHeapAddr));
		}

		#endregion

	}

}