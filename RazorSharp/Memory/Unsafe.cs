#region

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using InlineIL;
using JetBrains.Annotations;
using SimpleSharp.Diagnostics;
using RazorSharp.CoreClr;
using RazorSharp.CoreClr.Meta;
using RazorSharp.CoreClr.Structures;
using RazorSharp.CoreClr.Structures.EE;
using RazorSharp.Memory.Fixed;
using RazorSharp.Memory.Pointers;
using RazorSharp.Utilities;

#endregion

namespace RazorSharp.Memory
{
	#region

	using CSUnsafe = System.Runtime.CompilerServices.Unsafe;

	#endregion


	/// <summary>
	///     Provides utilities for manipulating pointers, memory, and types
	///     <seealso cref="BitConverter" />
	///     <seealso cref="System.Convert" />
	///     <seealso cref="MemoryMarshal" />
	///     <seealso cref="Marshal" />
	///     <seealso cref="Span{T}" />
	///     <seealso cref="Memory{T}" />
	///     <seealso cref="Buffer" />
	///     <seealso cref="CSUnsafe" />
	///     <seealso cref="System.Runtime.CompilerServices.JitHelpers" />
	///     <seealso cref="Mem" />
	/// </summary>
	public static unsafe class Unsafe
	{
		#region Other

		public static T Unbox<T>(object value) where T : struct
		{
			lock (value) {
				Pointer<byte> addr = AddressOfHeap(value, OffsetOptions.FIELDS);
				return addr.ReadAny<T>();
			}
		}

		public static object LoadFromEx(Type type, byte[] mem)
		{
			return ReflectionUtil.InvokeGenericMethod(typeof(Unsafe), nameof(LoadFrom),
			                                          null, new[] {type}, mem);
		}

		/// <summary>
		/// Creates an instance of <typeparamref name="T"/> from <paramref name="mem"/>.
		/// If <typeparamref name="T"/> is a reference type, <paramref name="mem"/> should not contain
		/// object internals like its <see cref="MethodTable"/> pointer or its <see cref="ObjHeader"/>; it should
		/// only contain its fields.
		/// </summary>
		/// <param name="mem">Memory to load from</param>
		/// <typeparam name="T">Type to load</typeparam>
		/// <returns>An instance created from <paramref name="mem"/></returns>
		public static T LoadFrom<T>(byte[] mem)
		{
			T             value = default;
			Pointer<byte> addr;

			if (Runtime.IsStruct<T>()) {
				addr = AddressOf(ref value).Cast<byte>();
			}
			else {
				value = Runtime.AllocObject<T>();
				addr  = AddressOfData(ref value).Cast<byte>();
			}

			addr.WriteAll(mem);

			return value;
		}

		/// <summary>
		///     Interprets a dynamically allocated reference type in the heap as a proper managed type. This is useful when
		///     you only have a pointer to a reference type's data in the heap but cannot dereference it because the CLR
		///     automatically dereferences managed reference types (pointer logistics is handled by the CLR).
		/// </summary>
		/// <param name="rawMem">Pointer to the reference type's raw data</param>
		/// <typeparam name="T">Type to interpret the data as</typeparam>
		/// <returns>A CLR-compliant reference type pointer to access the data pointed to by <paramref name="rawMem" /></returns>
		public static T RawInterpret<T>(Pointer<byte> rawMem) where T : class
		{
			var cpy = rawMem.Address;
			return CSUnsafe.Read<T>(&cpy);
		}

		/// <summary>
		/// Whether the value of <paramref name="value"/> is <c>default</c> or <c>null</c> bytes,
		/// or <paramref name="value"/> is <c>null</c>
		/// </summary>
		public static bool IsNil<T>([CanBeNull] T value)
		{
			return AddressOf(ref value).IsNil;
		}

		public static T DeepCopy<T>(T value) where T : class
		{
			Conditions.Require(!Runtime.IsString(value), nameof(value));
			Conditions.Require(!Runtime.IsArray(value), nameof(value));

			lock (value) {
				var valueCpy = GCHeap.AllocateObject<T>(0);

				fixed (byte* data = &PinHelper.GetPinningHelper(valueCpy).Data) {
					Pointer<byte> ptr = data;
					byte[]        mem = MemoryOfFields(value);
					ptr.WriteAll(mem);
				}

				return valueCpy;
			}
		}

		#endregion

		#region Address

		/// <summary>
		///     Returns the address of a field in the specified type.
		/// </summary>
		/// <param name="instance">Instance of the enclosing type</param>
		/// <param name="name">Name of the field</param>
		public static Pointer<byte> AddressOfField<T>(ref T instance, string name)
		{
			Pointer<FieldDesc> fd = typeof(T).GetFieldDesc(name);
			return fd.Reference.GetAddress(ref instance);
		}

		public static Pointer<byte> AddressOfField<T>(T instance, string name) where T : class
		{
			return AddressOfField(ref instance, name);
		}


		/// <summary>
		///     <para>Returns the address of <paramref name="value" />.</para>
		///     <remarks>
		///         <para>Equals <see cref="CSUnsafe.AsPointer{T}" /></para>
		///     </remarks>
		/// </summary>
		/// <param name="value">Type to return the address of</param>
		/// <returns>The address of the type in memory.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Pointer<T> AddressOf<T>(ref T value)
		{
			/*var tr = __makeref(t);
			return *(IntPtr*) (&tr);*/
			return CSUnsafe.AsPointer(ref value);
		}

		/// <summary>
		/// Returns the address of the data of <see cref="value"/>. If <typeparamref name="T"/> is a value type,
		/// this will return <see cref="AddressOf{T}"/>. If <typeparamref name="T"/> is a reference type,
		/// this will return the equivalent of <see cref="AddressOfHeap{T}(T, OffsetOptions)"/> with
		/// <see cref="OffsetOptions.FIELDS"/>.
		/// </summary>
		public static Pointer<byte> AddressOfData<T>(ref T value)
		{
			var addr = AddressOf(ref value);

			if (Runtime.IsStruct(value)) {
				return addr.Cast<byte>();
			}

			return addr.ReadPointer<byte>() + Offsets.OffsetToData;
		}

		/// <summary>
		///     Returns the address of reference type <paramref name="value" />'s heap memory (raw data).
		///     <remarks>
		///         <para>
		///             Note: This does not pin the reference in memory if it is a reference type.
		///             This may require pinning to prevent the GC from moving the object.
		///             If the GC compacts the heap, this pointer may become invalid.
		///         </para>
		///     </remarks>
		/// </summary>
		/// <param name="value">Reference type to return the heap address of</param>
		/// <returns>The address of the heap object.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Pointer<byte> AddressOfHeap<T>(T value) where T : class
		{
			var tr = __makeref(value);

			// NOTE:
			// Strings have their data offset by Offsets.OffsetToStringData
			// Arrays have their data offset by IntPtr.Size * 2 bytes (may be different for 32 bit)
			return **(IntPtr**) (&tr);
		}

		/// <summary>
		///     Returns the address of reference type <paramref name="value" />'s heap memory, offset by the specified
		///     <see cref="OffsetOptions" />.
		///     <remarks>
		///         <para>
		///             Note: This does not pin the reference in memory if it is a reference type.
		///             This may require pinning to prevent the GC from moving the object.
		///             If the GC compacts the heap, this pointer may become invalid.
		///         </para>
		///     </remarks>
		/// </summary>
		/// <param name="value">Reference type to return the heap address of</param>
		/// <param name="offset">Offset type</param>
		/// <returns>The address of <paramref name="value" /></returns>
		/// <exception cref="ArgumentOutOfRangeException">If <paramref name="offset"></paramref> is out of range.</exception>
		public static Pointer<byte> AddressOfHeap<T>(T value, OffsetOptions offset) where T : class
		{
			switch (offset) {
				case OffsetOptions.STRING_DATA:

					Conditions.Require(Runtime.IsString(value));
					string s = value as string;
					return AddressOfHeap(s) + Offsets.OffsetToStringData;

				case OffsetOptions.ARRAY_DATA:

					Conditions.Require(Runtime.IsArray(value));
					return AddressOfHeap(value) + Offsets.OffsetToArrayData;

				case OffsetOptions.FIELDS:

					// todo: if the type is an array, should this return ArrayData,
					// todo: ...and if it's a string, should this return StringData?

					// Skip over the MethodTable*
					return AddressOfHeap(value) + Offsets.OffsetToData;

				case OffsetOptions.NONE:
					return AddressOfHeap(value);

				case OffsetOptions.HEADER:
					return AddressOfHeap(value) - Offsets.OffsetToData;
				default:
					throw new ArgumentOutOfRangeException(nameof(offset), offset, null);
			}
		}

		public static Pointer<byte> AddressOfFunction<T>(string name)
		{
			return AddressOfFunction(typeof(T), name);
		}

		/// <summary>
		///     Returns the entry point of the specified function (assembly code).
		/// </summary>
		/// <param name="t">Enclosing type</param>
		/// <param name="name">Name of the function in <see cref="Type" /> <paramref name="t" /></param>
		/// <returns></returns>
		public static Pointer<byte> AddressOfFunction(Type t, string name)
		{
			Pointer<MethodDesc> md = t.GetMethodDesc(name);

			// Function must be jitted

			if (!md.Reference.IsPointingToNativeCode)
				md.Reference.Prepare();

			return md.Reference.Function;
		}

		#endregion

		#region Sizes

		
		public static int SizeOfAuto<T>(SizeOfOptions options = SizeOfOptions.Intrinsic)
		{
			return SizeOfAuto<T>(default, options);
		}

		public static int SizeOfAuto<T>(T value, SizeOfOptions options = SizeOfOptions.Intrinsic)
		{
//			if (Runtime.IsNullOrDefault(value) && options == SizeOfOptions.Intrinsic) { }

			var mt      = typeof(T).GetMethodTable();
			var eeClass = mt.Reference.EEClass;

			if (options == SizeOfOptions.Auto) {
				if (Runtime.IsStruct<T>()) {
					// Break into the next switch branch which will go to case Intrinsic
					options = SizeOfOptions.Intrinsic;
				}
				else {
					// Break into the next switch branch which will go to case Heap
					options = SizeOfOptions.Heap;
				}
			}

			// If a value was supplied
			if (!Runtime.IsNullOrDefault(value)) {
				mt = value.GetType().GetMethodTable();

				switch (options) {
					case SizeOfOptions.BaseFields:   return mt.Reference.NumInstanceFieldBytes;
					case SizeOfOptions.BaseInstance: return mt.Reference.BaseSize;
					case SizeOfOptions.Heap:         return HeapSizeInternal(value);
				}
			}

			switch (options) {
				// Note: Arrays native size == 0
				case SizeOfOptions.Native: return eeClass.Reference.NativeSize;
				// Note: Arrays have no layout
				case SizeOfOptions.Managed:
					if (eeClass.Reference.HasLayout)
						return (int) eeClass.Reference.LayoutInfo.Reference.ManagedSize;
					else {
						return Constants.INVALID_VALUE;
					}

				case SizeOfOptions.Intrinsic:  return CSUnsafe.SizeOf<T>();
				case SizeOfOptions.BaseFields: return mt.Reference.NumInstanceFieldBytes;
				case SizeOfOptions.BaseInstance:
					Conditions.Require(!Runtime.IsStruct<T>(), nameof(value));
					return mt.Reference.BaseSize;
				case SizeOfOptions.Heap:
					throw new ArgumentException($"A value must be supplied to use {SizeOfOptions.Heap}");

//				default:
//					throw new ArgumentOutOfRangeException(nameof(options), options, null);
			}


			return Constants.INVALID_VALUE;
		}

		/// <summary>
		///     <para>Returns the size of a type in memory.</para>
		///     <para>Call to <see cref="CSUnsafe.SizeOf{T}()" /></para>
		/// </summary>
		/// <returns><see cref="IntPtr.Size" /> for reference types, size for value types</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int SizeOf<T>() => CSUnsafe.SizeOf<T>();


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
		///     <para>Equals <see cref="SizeOfAuto{T}(T,SizeOfOptions)" /> with <see cref="SizeOfOptions.BaseInstance"/> for objects that aren't arrays or strings.</para>
		///     <para>Note: This also includes padding and overhead (<see cref="ObjHeader" /> and <see cref="MethodTable" /> ptr.)</para>
		/// </remarks>
		/// <returns>The size of the type in heap memory, in bytes</returns>
		public static int HeapSize<T>(T value) where T : class
			=> HeapSizeInternal(value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int HeapSizeInternal<T>(T value)
		{
			// Sanity check
			Conditions.Require(!Runtime.IsStruct<T>());


			if (value == null) {
				return Constants.INVALID_VALUE;
			}

			// By manually reading the MethodTable*, we can calculate the size correctly if the reference
			// is boxed or cloaked
			Pointer<MethodTable> methodTable = Runtime.ReadMethodTable(ref value);

			// Value of GetSizeField()
			int length = 0;

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

			if (Runtime.IsArray(value)) {
				var arr = value as Array;

				// ReSharper disable once PossibleNullReferenceException
				// We already know it's not null because the type is an array.
				length = arr.Length;

				// Sanity check
				Conditions.Assert(!Runtime.IsString(value));
			}
			else if (Runtime.IsString(value)) {
				string str = value as string;

				// Sanity check
				Conditions.Assert(!Runtime.IsArray(value));
				Conditions.NotNull(str, nameof(str));

				length = str.Length;
			}

			return methodTable.Reference.BaseSize + length * methodTable.Reference.ComponentSize;
		}

		#endregion

		#region Size of data

		/// <summary>
		///     Returns the size of the data in <paramref name="value"/>. If <typeparamref name="T"/> is a reference type,
		/// this returns the size of <paramref name="value"/> not occupied by the <see cref="MethodTable" /> pointer and the <see cref="ObjHeader" />.
		/// If <typeparamref name="T"/> is a value type, this returns <see cref="SizeOf{T}"/>.
		/// </summary>
		public static int SizeOfData<T>(T value)
		{
			if (Runtime.IsStruct(value)) {
				return SizeOf<T>();
			}
			else {
				// Subtract the size of the ObjHeader and MethodTable*
				return HeapSizeInternal(value) - (IntPtr.Size + sizeof(MethodTable*));
			}
		}

		/// <summary>
		///     Returns the base size of the data in the type specified by <paramref name="t"/>. If <paramref name="t"/> is a reference type,
		/// this returns the size of data not occupied by the <see cref="MethodTable" /> pointer, <see cref="ObjHeader" />, padding, and overhead.
		/// If <paramref name="t"/> is a value type, this returns <see cref="SizeOf{T}"/>.
		/// </summary>
		public static int BaseSizeOfData(Type t)
		{
			if (t.IsValueType) {
				return (int) ReflectionUtil.InvokeGenericMethod(typeof(Unsafe), nameof(SizeOf), null, new[] {t}, null);
			}
			else {
				// Subtract the size of the ObjHeader and MethodTable*
				return t.GetMetaType().NumInstanceFieldBytes;
			}
		}

		#endregion

		#endregion

		#region Misc

		public static bool IsBoxed<T>(in T value)
		{
			return (typeof(T).IsInterface || typeof(T) == typeof(object))
			       && value != null
			       && Runtime.IsStruct(value);
		}


		/// <summary>
		///     Copies the memory of <paramref name="value" /> into a <see cref="Byte" /> array.
		///     <remarks>
		///         This includes the <see cref="MethodTable" /> pointer and <see cref="ObjHeader" />
		///     </remarks>
		/// </summary>
		/// <param name="value">Value to copy the memory of</param>
		/// <typeparam name="T">Reference type</typeparam>
		/// <returns>An array of <see cref="Byte" />s containing the raw memory of <paramref name="value" /></returns>
		public static byte[] MemoryOf<T>(T value) where T : class
		{
			// Need to include the ObjHeader
			Pointer<T> ptr = AddressOfHeap(value, OffsetOptions.HEADER).Address;
			return ptr.Cast<byte>().CopyOut(HeapSize(value));
		}

		/// <summary>
		///     Copies the memory of <paramref name="value" /> into a <see cref="Byte" /> array.
		///     If <typeparamref name="T" /> is a pointer, the memory of the pointer will be returned.
		/// </summary>
		/// <param name="value">Value to copy the memory of</param>
		/// <typeparam name="T">Value type</typeparam>
		/// <returns>An array of <see cref="Byte" />s containing the raw memory of <paramref name="value" /></returns>
		public static byte[] MemoryOfVal<T>(T value)
		{
			Pointer<T> ptr = AddressOf(ref value);
			return ptr.Cast<byte>().CopyOut(ptr.ElementSize);
		}

		public static byte[] MemoryOfFields<T>(T value) where T : class
		{
			int fieldSize = SizeOfData(value);
			var fields    = new byte[fieldSize];

			// Skip over the MethodTable*
			Marshal.Copy((AddressOfHeap(value) + IntPtr.Size).Address, fields, 0, fieldSize);
			return fields;
		}

		#endregion
	}
}