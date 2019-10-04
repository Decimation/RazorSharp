#region

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using InlineIL;
using RazorSharp.CoreClr;
using RazorSharp.CoreClr.Meta;
using RazorSharp.CoreClr.Metadata;
using RazorSharp.Interop;
using RazorSharp.Memory.Enums;
using RazorSharp.Memory.Pointers;
using SimpleSharp.Diagnostics;

// ReSharper disable UnusedParameter.Global
// ReSharper disable CommentTypo

#endregion

// ReSharper disable SwitchStatementMissingSomeCases

namespace RazorSharp.Memory
{
	/// <summary>
	///     Provides utilities for manipulating pointers, memory, and types. This class has CompilerServices's
	/// 	Unsafe built in.
	///     <seealso cref="BitConverter" />
	///     <seealso cref="System.Convert" />
	///     <seealso cref="MemoryMarshal" />
	///     <seealso cref="Marshal" />
	///     <seealso cref="Span{T}" />
	///     <seealso cref="Memory{T}" />
	///     <seealso cref="Buffer" />
	///     <seealso cref="System.Runtime.CompilerServices.JitHelpers" />
	///     <seealso cref="Mem" />
	/// </summary>
	public static unsafe class Unsafe
	{
		#region Other

		public static T UnboxRaw<T>(object value) where T : struct
		{
			lock (value) {
				Pointer<byte> addr = AddressOfHeap(value, OffsetOptions.Fields);
				return addr.Cast<T>().Read();
			}
		}

		/*public static T DeepCopy<T>(T value) where T : class
		{
			Conditions.Require(!RtInfo.IsArrayOrString(value), nameof(value));

			lock (value) {
				var valueCpy = GlobalHeap.AllocateObject<T>(0);

				fixed (byte* data = &PinHelper.GetPinningHelper(valueCpy).Data) {
					Pointer<byte> ptr = data;
					byte[]        mem = MemoryOfFields(value);
					ptr.WriteAll(mem);
				}

				return valueCpy;
			}
		}*/

		#endregion

		#region Address

		/// <summary>
		///     <para>Returns the address of <paramref name="value" />.</para>
		///     <remarks>
		///         <para>Equals <see cref="AsPointer{T}" /></para>
		///     </remarks>
		/// </summary>
		/// <param name="value">Type to return the address of</param>
		/// <returns>The address of the type in memory.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Pointer<T> AddressOf<T>(ref T value)
		{
			/*var tr = __makeref(t);
			return *(IntPtr*) (&tr);*/
			return AsPointer(ref value);
		}

		/// <summary>
		///     Returns the address of the data of <paramref name="value"/>. If <typeparamref name="T" /> is a value type,
		///     this will return <see cref="AddressOf{T}" />. If <typeparamref name="T" /> is a reference type,
		///     this will return the equivalent of <see cref="AddressOfHeap{T}(T, OffsetOptions)" /> with
		///     <see cref="OffsetOptions.Fields" />.
		/// </summary>
		public static Pointer<byte> AddressOfFields<T>(ref T value)
		{
			Pointer<T> addr = AddressOf(ref value);

			if (RuntimeInfo.IsStruct(value)) {
				return addr.Cast();
			}

			return AddressOfHeapInternal(value, OffsetOptions.Fields);
		}

		public static bool TryGetAddressOfHeap<T>(T value, OffsetOptions options, out Pointer<byte> ptr)
		{
			if (RuntimeInfo.IsStruct(value)) {
				ptr = null;
				return false;
			}

			ptr = AddressOfHeapInternal(value, options);
			return true;
		}

		public static bool TryGetAddressOfHeap<T>(T value, out Pointer<byte> ptr)
		{
			return TryGetAddressOfHeap(value, OffsetOptions.None, out ptr);
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
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Pointer<byte> AddressOfHeap<T>(T value, OffsetOptions offset = OffsetOptions.None) where T : class
			=> AddressOfHeapInternal(value, offset);

		private static Pointer<byte> AddressOfHeapInternal<T>(T value, OffsetOptions offset)
		{
			// It is already assumed value is a class type

			//var tr = __makeref(value);
			//var heapPtr = **(IntPtr**) (&tr);

			var heapPtr = AddressOf(ref value).ReadPointer();


			// NOTE:
			// Strings have their data offset by Offsets.OffsetToStringData
			// Arrays have their data offset by IntPtr.Size * 2 bytes (may be different for 32 bit)


			switch (offset) {
				case OffsetOptions.StringData:
				{
					Conditions.Require(RuntimeInfo.IsString(value));
					string s = value as string;
					return heapPtr + Offsets.OffsetToStringData;
				}

				case OffsetOptions.ArrayData:
				{
					Conditions.Require(RuntimeInfo.IsArray(value));
					return heapPtr + Offsets.OffsetToArrayData;
				}

				case OffsetOptions.Fields:
				{
					// todo: if the type is an array, should this return ArrayData, ...and if it's a string,
					// ... should this return StringData?

					// Skip over the MethodTable*
					return heapPtr + Offsets.OffsetToData;
				}

				case OffsetOptions.None:
					return heapPtr;

				case OffsetOptions.Header:
					return heapPtr - Offsets.OffsetToData;
				default:
					throw new ArgumentOutOfRangeException(nameof(offset), offset, null);
			}
		}

		#endregion

		#region Sizes

		public static int SizeOf<T>(SizeOfOptions options)
		{
			return SizeOf<T>(default, options);
		}

		public static int SizeOf<T>(T value, SizeOfOptions options = SizeOfOptions.Intrinsic)
		{
			MetaType mt = typeof(T);

			if (options == SizeOfOptions.Auto) {
				// Break into the next switch branch which will go to resolved case
				options = RuntimeInfo.IsStruct(value) ? SizeOfOptions.Intrinsic : SizeOfOptions.Heap;
			}

			// If a value was supplied
			if (!RuntimeInfo.IsNil(value)) {
				mt = new MetaType(value.GetType());

				switch (options) {
					case SizeOfOptions.BaseFields:   return mt.InstanceFieldsSize;
					case SizeOfOptions.BaseInstance: return mt.BaseSize;
					case SizeOfOptions.Heap:         return HeapSizeInternal(value);
					case SizeOfOptions.Data:         return SizeOfData(value);
					case SizeOfOptions.BaseData:     return BaseSizeOfData(mt.RuntimeType);
				}
			}


			switch (options) {
				// Note: Arrays native size == 0
				case SizeOfOptions.Native: return mt.NativeSize;

				// Note: Arrays have no layout
				case SizeOfOptions.Managed:
				{
					return mt.HasLayout ? mt.LayoutInfo.ManagedSize : Constants.INVALID_VALUE;
				}

				case SizeOfOptions.Intrinsic: return SizeOf<T>();

				case SizeOfOptions.BaseFields: return mt.InstanceFieldsSize;

				case SizeOfOptions.BaseInstance:
				{
					Conditions.Require(!RuntimeInfo.IsStruct<T>());
					return mt.BaseSize;
				}

				case SizeOfOptions.Heap:
				case SizeOfOptions.Data:
					throw new ArgumentException($"A value must be supplied to use {options}");
			}


			return Constants.INVALID_VALUE;
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
		///                 (<c>24</c> (x64) or <c>12</c> (x86) by default) (<see cref="Offsets.MinObjectSize" />)
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
		///     <para>
		///         Equals <see cref="SizeOf{T}(T,SizeOfOptions)" /> with <see cref="SizeOfOptions.BaseInstance" /> for objects
		///         that aren't arrays or strings.
		///     </para>
		///     <para>Note: This also includes padding and overhead (<see cref="ObjHeader" /> and <see cref="MethodTable" /> ptr.)</para>
		/// </remarks>
		/// <returns>The size of the type in heap memory, in bytes</returns>
		public static int HeapSize<T>(T value) where T : class
			=> HeapSizeInternal(value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int HeapSizeInternal<T>(T value)
		{
			// Sanity check
			Conditions.Require(!RuntimeInfo.IsStruct(value));

			if (RuntimeInfo.IsNil(value)) {
				return Constants.INVALID_VALUE;
			}

			// By manually reading the MethodTable*, we can calculate the size correctly if the reference
			// is boxed or cloaked
			var methodTable = Runtime.ReadMetaType(value);

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

			if (RuntimeInfo.IsArray(value)) {
				var arr = value as Array;

				// ReSharper disable once PossibleNullReferenceException
				// We already know it's not null because the type is an array.
				length = arr.Length;

				// Sanity check
				Conditions.Assert(!RuntimeInfo.IsString(value));
			}
			else if (RuntimeInfo.IsString(value)) {
				string str = value as string;

				// Sanity check
				Conditions.Assert(!RuntimeInfo.IsArray(value));
				Conditions.NotNull(str, nameof(str));

				length = str.Length;
			}

			return methodTable.BaseSize + length * methodTable.ComponentSize;
		}

		#endregion

		#region Size of data

		/// <summary>
		///     Returns the size of the data in <paramref name="value" />. If <typeparamref name="T" /> is a reference type,
		///     this returns the size of <paramref name="value" /> not occupied by the <see cref="MethodTable" /> pointer and the
		///     <see cref="ObjHeader" />.
		///     If <typeparamref name="T" /> is a value type, this returns <see cref="SizeOf{T}()" />.
		/// </summary>
		private static int SizeOfData<T>(T value)
		{
			if (RuntimeInfo.IsStruct(value)) {
				return SizeOf<T>();
			}

			// Subtract the size of the ObjHeader and MethodTable*
			return HeapSizeInternal(value) - Offsets.ObjectOverhead;
		}


		/// <summary>
		///     Returns the base size of the data in the type specified by <paramref name="t" />. If <paramref name="t" /> is a
		///     reference type,
		///     this returns the size of data not occupied by the <see cref="MethodTable" /> pointer, <see cref="ObjHeader" />,
		///     padding, and overhead.
		///     If <paramref name="t" /> is a value type, this returns <see cref="SizeOf{T}()" />.
		/// </summary>
		private static int BaseSizeOfData(Type t)
		{
			if (((MetaType) t).IsStruct) {
				return (int) Functions.CallGenericMethod(typeof(Unsafe).GetMethod(nameof(SizeOf)), t, null);
			}

			// Subtract the size of the ObjHeader and MethodTable*
			return new MetaType(t).InstanceFieldsSize;
		}

		#endregion

		#endregion

		#region Misc

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
			Pointer<T> ptr = AddressOfHeap(value, OffsetOptions.Header);
			return ptr.Cast().Copy(HeapSize(value));
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
			return ptr.Cast().Copy(ptr.ElementSize);
		}

		public static byte[] MemoryOfFields<T>(T value) where T : class
		{
			int fieldSize = SizeOfData(value);
			var fields    = new byte[fieldSize];

			// Skip over the MethodTable*
			Marshal.Copy(AddressOfHeap(value, OffsetOptions.Fields).Address, fields, 0, fieldSize);
			return fields;
		}

		#endregion

		#region Unsafe

		// https://github.com/ltrzesniewski/InlineIL.Fody/blob/master/src/InlineIL.Examples/Unsafe.cs
		// https://github.com/dotnet/corefx/blob/master/src/System.Runtime.CompilerServices.Unsafe/src/System.Runtime.CompilerServices.Unsafe.il


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T Read<T>(void* source)
		{
			IL.Emit.Ldarg(nameof(source));
			IL.Emit.Ldobj(typeof(T));
			return IL.Return<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T ReadUnaligned<T>(void* source)
		{
			IL.Emit.Ldarg(nameof(source));
			IL.Emit.Unaligned(1);
			IL.Emit.Ldobj(typeof(T));
			return IL.Return<T>();
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T ReadUnaligned<T>(ref byte source)
		{
			IL.Emit.Ldarg(nameof(source));
			IL.Emit.Unaligned(1);
			IL.Emit.Ldobj(typeof(T));
			return IL.Return<T>();
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Write<T>(void* destination, T value)
		{
			IL.Emit.Ldarg(nameof(destination));
			IL.Emit.Ldarg(nameof(value));
			IL.Emit.Stobj(typeof(T));
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void WriteUnaligned<T>(void* destination, T value)
		{
			IL.Emit.Ldarg(nameof(destination));
			IL.Emit.Ldarg(nameof(value));
			IL.Emit.Unaligned(1);
			IL.Emit.Stobj(typeof(T));
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void WriteUnaligned<T>(ref byte destination, T value)
		{
			IL.Emit.Ldarg(nameof(destination));
			IL.Emit.Ldarg(nameof(value));
			IL.Emit.Unaligned(1);
			IL.Emit.Stobj(typeof(T));
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Copy<T>(void* destination, ref T source)
		{
			IL.Emit.Ldarg(nameof(destination));
			IL.Emit.Ldarg(nameof(source));
			IL.Emit.Ldobj(typeof(T));
			IL.Emit.Stobj(typeof(T));
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Copy<T>(ref T destination, void* source)
		{
			IL.Emit.Ldarg(nameof(destination));
			IL.Emit.Ldarg(nameof(source));
			IL.Emit.Ldobj(typeof(T));
			IL.Emit.Stobj(typeof(T));
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void* AsPointer<T>(ref T value)
		{
			IL.Emit.Ldarg(nameof(value));
			IL.Emit.Conv_U();
			return IL.ReturnPointer();
		}


		/// <summary>
		///     <para>Returns the size of a type in memory.</para>
		/// </summary>
		/// <returns><see cref="IntPtr.Size" /> for reference types, size for value types</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int SizeOf<T>()
		{
			IL.Emit.Sizeof(typeof(T));
			return IL.Return<int>();
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void CopyBlock(void* destination, void* source, uint byteCount)
		{
			IL.Emit.Ldarg(nameof(destination));
			IL.Emit.Ldarg(nameof(source));
			IL.Emit.Ldarg(nameof(byteCount));
			IL.Emit.Cpblk();
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void CopyBlock(ref byte destination, ref byte source, uint byteCount)
		{
			IL.Emit.Ldarg(nameof(destination));
			IL.Emit.Ldarg(nameof(source));
			IL.Emit.Ldarg(nameof(byteCount));
			IL.Emit.Cpblk();
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void CopyBlockUnaligned(void* destination, void* source, uint byteCount)
		{
			IL.Emit.Ldarg(nameof(destination));
			IL.Emit.Ldarg(nameof(source));
			IL.Emit.Ldarg(nameof(byteCount));
			IL.Emit.Unaligned(1);
			IL.Emit.Cpblk();
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void CopyBlockUnaligned(ref byte destination, ref byte source, uint byteCount)
		{
			IL.Emit.Ldarg(nameof(destination));
			IL.Emit.Ldarg(nameof(source));
			IL.Emit.Ldarg(nameof(byteCount));
			IL.Emit.Unaligned(1);
			IL.Emit.Cpblk();
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void InitBlock(void* startAddress, byte value, uint byteCount)
		{
			IL.Emit.Ldarg(nameof(startAddress));
			IL.Emit.Ldarg(nameof(value));
			IL.Emit.Ldarg(nameof(byteCount));
			IL.Emit.Initblk();
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void InitBlock(ref byte startAddress, byte value, uint byteCount)
		{
			IL.Emit.Ldarg(nameof(startAddress));
			IL.Emit.Ldarg(nameof(value));
			IL.Emit.Ldarg(nameof(byteCount));
			IL.Emit.Initblk();
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void InitBlockUnaligned(void* startAddress, byte value, uint byteCount)
		{
			IL.Emit.Ldarg(nameof(startAddress));
			IL.Emit.Ldarg(nameof(value));
			IL.Emit.Ldarg(nameof(byteCount));
			IL.Emit.Unaligned(1);
			IL.Emit.Initblk();
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void InitBlockUnaligned(ref byte startAddress, byte value, uint byteCount)
		{
			IL.Emit.Ldarg(nameof(startAddress));
			IL.Emit.Ldarg(nameof(value));
			IL.Emit.Ldarg(nameof(byteCount));
			IL.Emit.Unaligned(1);
			IL.Emit.Initblk();
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T As<T>(object o) where T : class
		{
			IL.Emit.Ldarg(nameof(o));
			return IL.Return<T>();
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T AsRef<T>(void* source)
		{
			// For .NET Core the roundtrip via a local is no longer needed (update the constant as needed)
#if NETCOREAPP
            IL.Push(source);
            return ref IL.ReturnRef<T>();
#else
			// Roundtrip via a local to avoid type mismatch on return that the JIT inliner chokes on.
			IL.DeclareLocals(
				false,
				new LocalVar("local", typeof(int).MakeByRefType())
			);

			IL.Push(source);
			IL.Emit.Stloc("local");
			IL.Emit.Ldloc("local");
			return ref IL.ReturnRef<T>();
#endif
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T AsRef<T>(in T source)
		{
			IL.Emit.Ldarg(nameof(source));
			return ref IL.ReturnRef<T>();
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref TTo As<TFrom, TTo>(ref TFrom source)
		{
			IL.Emit.Ldarg(nameof(source));
			return ref IL.ReturnRef<TTo>();
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T Unbox<T>(object box) where T : struct
		{
			IL.Push(box);
			IL.Emit.Unbox(typeof(T));
			return ref IL.ReturnRef<T>();
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T Add<T>(ref T source, int elementOffset)
		{
			IL.Emit.Ldarg(nameof(source));
			IL.Emit.Ldarg(nameof(elementOffset));
			IL.Emit.Sizeof(typeof(T));
			IL.Emit.Conv_I();
			IL.Emit.Mul();
			IL.Emit.Add();
			return ref IL.ReturnRef<T>();
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void* Add<T>(void* source, int elementOffset)
		{
			IL.Emit.Ldarg(nameof(source));
			IL.Emit.Ldarg(nameof(elementOffset));
			IL.Emit.Sizeof(typeof(T));
			IL.Emit.Conv_I();
			IL.Emit.Mul();
			IL.Emit.Add();
			return IL.ReturnPointer();
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T Add<T>(ref T source, IntPtr elementOffset)
		{
			IL.Emit.Ldarg(nameof(source));
			IL.Emit.Ldarg(nameof(elementOffset));
			IL.Emit.Sizeof(typeof(T));
			IL.Emit.Mul();
			IL.Emit.Add();
			return ref IL.ReturnRef<T>();
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T AddByteOffset<T>(ref T source, IntPtr byteOffset)
		{
			IL.Emit.Ldarg(nameof(source));
			IL.Emit.Ldarg(nameof(byteOffset));
			IL.Emit.Add();
			return ref IL.ReturnRef<T>();
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T Subtract<T>(ref T source, int elementOffset)
		{
			IL.Emit.Ldarg(nameof(source));
			IL.Emit.Ldarg(nameof(elementOffset));
			IL.Emit.Sizeof(typeof(T));
			IL.Emit.Conv_I();
			IL.Emit.Mul();
			IL.Emit.Sub();
			return ref IL.ReturnRef<T>();
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void* Subtract<T>(void* source, int elementOffset)
		{
			IL.Emit.Ldarg(nameof(source));
			IL.Emit.Ldarg(nameof(elementOffset));
			IL.Emit.Sizeof(typeof(T));
			IL.Emit.Conv_I();
			IL.Emit.Mul();
			IL.Emit.Sub();
			return IL.ReturnPointer();
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T Subtract<T>(ref T source, IntPtr elementOffset)
		{
			IL.Emit.Ldarg(nameof(source));
			IL.Emit.Ldarg(nameof(elementOffset));
			IL.Emit.Sizeof(typeof(T));
			IL.Emit.Mul();
			IL.Emit.Sub();
			return ref IL.ReturnRef<T>();
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T SubtractByteOffset<T>(ref T source, IntPtr byteOffset)
		{
			IL.Emit.Ldarg(nameof(source));
			IL.Emit.Ldarg(nameof(byteOffset));
			IL.Emit.Sub();
			return ref IL.ReturnRef<T>();
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IntPtr ByteOffset<T>(ref T origin, ref T target)
		{
			IL.Emit.Ldarg(nameof(target));
			IL.Emit.Ldarg(nameof(origin));
			IL.Emit.Sub();
			return IL.Return<IntPtr>();
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool AreSame<T>(ref T left, ref T right)
		{
			IL.Emit.Ldarg(nameof(left));
			IL.Emit.Ldarg(nameof(right));
			IL.Emit.Ceq();
			return IL.Return<bool>();
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsAddressGreaterThan<T>(ref T left, ref T right)
		{
			IL.Emit.Ldarg(nameof(left));
			IL.Emit.Ldarg(nameof(right));
			IL.Emit.Cgt_Un();
			return IL.Return<bool>();
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsAddressLessThan<T>(ref T left, ref T right)
		{
			IL.Emit.Ldarg(nameof(left));
			IL.Emit.Ldarg(nameof(right));
			IL.Emit.Clt_Un();
			return IL.Return<bool>();
		}

		#endregion
	}
}